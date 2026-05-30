using System.Collections.Immutable;
using FlowTrack.Iam.Auth.Domain;
using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Query;
using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Iam.Auth.Application.Signin;

public sealed class SigninQryHandler(
    IUserRepository repository,
    IBcrypt bcrypt,
    IEnvStore envStore,
    IJWTService jWTService
) : IQueryHandler<SigninQry, SigninSuccess>
{
    private readonly IUserRepository _repository = repository;
    private readonly IBcrypt _bcrypt = bcrypt;
    private readonly IEnvStore _envStore = envStore;

    private readonly IJWTService _jWTService = jWTService;

    public async Task<SigninSuccess> Handle(SigninQry qry)
    {
        var user = await _repository.FindByEmail(qry.Email);
        if (user is null)
        {
            throw new SigninFailed();
        }

        var isValidPassword = _bcrypt.Compare(qry.Password, user.Password);
        if (!isValidPassword)
        {
            throw new SigninFailed();
        }

        var accessTokenSecret =
            _envStore.Get("ACCESS_TOKEN_SECRET")
            ?? throw new EnvVariableMissed("ACCESS_TOKEN_SECRET");
        var accessTokenExpireIn = _envStore.Get("ACCESS_TOKEN_EXPIRE_MINUTES") ?? "10";

        var payload = new JWTPayload(
            new Dictionary<string, string> { { "id", user.Id.ToString() } }.ToImmutableDictionary()
        );

        var accessTokenOptions = new JWTOptions(accessTokenSecret, int.Parse(accessTokenExpireIn));

        var accessToken = _jWTService.Generate(payload, accessTokenOptions);

        var refreshTokenSecret = _envStore.Get("REFRESH_TOKEN_SECRET");
        var refreshTokenExpireIn = _envStore.Get("REFRESH_TOKEN_EXPIRE_MINUTES");

        var refreshToken = _jWTService.Generate(
            payload,
            new JWTOptions(refreshTokenSecret ?? "", int.Parse(refreshTokenExpireIn ?? "10"))
        );

        return new SigninSuccess(accessToken, refreshToken);
    }
}
