namespace FlowTrack.Iam.Users.Application;

[Service(Lifetime.Scoped)]
public sealed class FindUserByIdQryHandler(IUserRepository repository)
    : IQueryHandler<FindUserByIdQry, User>
{
    public async Task<User> Handle(FindUserByIdQry query)
    {
        User? user = await repository.FindById(query.Id) ?? throw new UserNotFound();

        return user;
    }
}
