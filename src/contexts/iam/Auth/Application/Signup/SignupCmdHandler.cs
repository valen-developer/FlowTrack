namespace FlowTrack.Iam.Auth.Application
{
    [Service]
    internal sealed class SignupCmdHandler(
        IUserRepository repository,
        EventBus eventBus,
        IBcrypt bcrypt
    ) : ICommandHandler<SignupCmd>
    {
        public async Task Handle(SignupCmd command)
        {
            var currentUser = await repository.FindByEmail(command.Email);
            if (currentUser is not null)
            {
                return;
            }

            Password password = Password.EnsurePassword(command.Password);
            UserEmail email = new(command.Email);

            string hashedPassword = bcrypt.Hash(password.Value);

            var user = User.Create(
                id: new UserId(command.Id),
                password: new UserPassword(hashedPassword),
                email: email
            );
            await repository.Create(user);

            await eventBus.Publish(user.PullDomainEvents());
        }
    }
}
