using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Iam.Application;

public sealed class FindUserByIdQryHandler(IUserRepository repository)
    : IQueryHandler<FindUserByIdQry, User>
{
    public async Task<User> Handle(FindUserByIdQry query)
    {
        User? user = await repository.FindById(query.Id) ?? throw new UserNotFound();

        return user;
    }
}
