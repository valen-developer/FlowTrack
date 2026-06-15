namespace FlowTrack.Iam.Auth.Application
{
    [Service(Lifetime.Scoped)]
    internal sealed class SigninQryHandler(
        IUserRepository repository,
        AuthTokenGenerator authTokenGenerator,
        IBcrypt bcrypt
    ) : IQueryHandler<SigninQry, SigninSuccess>
    {
        public async Task<SigninSuccess> Handle(SigninQry qry)
        {
            var user = await repository.FindByEmail(qry.Email) ?? throw new SigninFailed();

            if (!user.IsActive)
                throw new SigninFailed();

            var isValidPassword = bcrypt.Compare(qry.Password, user.Password.Value);
            if (!isValidPassword)
                throw new SigninFailed();

            var signinSuccess = authTokenGenerator.Generate(user);

            return signinSuccess;
        }
    }
}
