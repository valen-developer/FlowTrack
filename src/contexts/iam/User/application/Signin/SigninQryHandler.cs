using Iam.User.Domain;
using Shared.domain;
using Shared.Domain.Bus.Query;

namespace Iam.User.application.Signin;

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
