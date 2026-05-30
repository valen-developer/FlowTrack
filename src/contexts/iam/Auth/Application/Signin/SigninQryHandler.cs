using System.Collections.Immutable;
using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Auth.Application.Signin;

public sealed class SigninQryHandler(
    IUserRepository repository,
    IBcrypt bcrypt,
    IEnvStore envStore,
    IJWTService jWTService
) : IQueryHandler<SigninQry, Object>
{
    private readonly IUserRepository _repository = repository;
    private readonly IBcrypt _bcrypt = bcrypt;
    private readonly IEnvStore _envStore = envStore;

    private readonly IJWTService _jWTService = jWTService;

    public async Task<Object> Handle(SigninQry qry)
    {
        var user = await _repository.FindByEmail(qry.Email);
        _bcrypt.Compare(qry.Password, user.Password);

        var accessTokenSecret = _envStore.Get("ACCESS_TOKEN_SECRET");
        var accessTokenExpireIn = _envStore.Get("ACCESS_TOKEN_EXPIRE_MINUTES");

        var payload = new JWTPayload(
            new Dictionary<string, string> { { "id", user.Id.ToString() } }.ToImmutableDictionary()
        );

        var accessTokenOptions = new JWTOptions(
            accessTokenSecret ?? "",
            int.Parse(accessTokenExpireIn ?? "10")
        );

        _jWTService.Generate(payload, accessTokenOptions);

        _envStore.Get("REFRESH_TOKEN_SECRET");
        _envStore.Get("REFRESH_TOKEN_EXPIRE_MINUTES");

        return new Object();
    }
}
