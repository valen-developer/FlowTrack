using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Auth.Application.Signin;

public sealed class SigninQryHandler(IUserRepository repository, IBcrypt bcrypt, IEnvStore envStore)
    : IQueryHandler<SigninQry, Object>
{
    private readonly IUserRepository _repository = repository;
    private readonly IBcrypt _bcrypt = bcrypt;
    private readonly IEnvStore _envStore = envStore;

    public async Task<Object> Handle(SigninQry qry)
    {
        var user = await _repository.FindByEmail(qry.Email);
        _bcrypt.Compare(qry.Password, user.Password);

        _envStore.Get("ACCESS_TOKEN_SECRET");

        return new Object();
    }
}
