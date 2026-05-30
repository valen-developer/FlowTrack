using FlowTrack.Iam.User.Domain;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Auth.Application.Signin;

public sealed class SigninQryHandler(IUserRepository repository, IBcrypt bcrypt)
    : IQueryHandler<SigninQry, Object>
{
    private readonly IUserRepository _repository = repository;
    private readonly IBcrypt _bcrypt = bcrypt;

    public async Task<Object> Handle(SigninQry qry)
    {
        var user = await _repository.FindByEmail(qry.Email);
        _bcrypt.Compare(qry.Password, user.Password);

        return new Object();
    }
}
