using FlowTrack.Iam.Application;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;

namespace FlowTrack.Iam.Test.Infrastructure;

public class SignupCmdHandlerIT : IamIntegrationTestCase
{
    public SignupCmdHandlerIT()
        : base()
    {
        AddScoped<SignupCmdHandler, SignupCmdHandler>();

        AddScoped<DomainEventSubscriberScanner, DomainEventSubscriberScanner>();
        AddScoped<DomainEventDispatcher, DomainEventDispatcher>();
        AddScoped<IDomainEventBus, InMemoryDomainEventBus>();
    }

    [Fact]
    public async Task Should_Save_User()
    {
        var userDao = GetService<UserDao>();
        var handler = GetService<SignupCmdHandler>();

        var user = UserMother.Inactive();
        var cmd = new SignupCmd(user.Id.ToString(), user.Email, user.Password);

        await handler.Handle(cmd);

        var savedUser = await userDao.FindById(user.Id);

        Assert.Equivalent(user, savedUser);
    }
}
