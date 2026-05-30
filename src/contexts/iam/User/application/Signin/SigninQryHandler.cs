using Iam.User.Domain;
using Shared.Domain.Bus.Query;

namespace Iam.User.application.Signin;

public sealed class SigninQryHandler(IUserRepository repository) : IQueryHandler<SigninQry, Object>
{
    private readonly IUserRepository _repository = repository;

    public async Task<Object> Handle(SigninQry qry)
    {
        _repository.FindByEmail(qry.Email);

        return new Object();
    }
}
