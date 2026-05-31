using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Application;

public sealed class SigninQryHandler(
    IUserRepository repository,
    AuthTokenGenerator authTokenGenerator,
    IBcrypt bcrypt
) : IQueryHandler<SigninQry, SigninSuccess>
{
    public async Task<SigninSuccess> Handle(SigninQry qry)
    {
        var user = await repository.FindByEmail(qry.Email) ?? throw new SigninFailed();

        var isValidPassword = bcrypt.Compare(qry.Password, user.Password);
        if (!isValidPassword)
            throw new SigninFailed();

        var signinSuccess = authTokenGenerator.Generate(user);

        return signinSuccess;
    }
}
