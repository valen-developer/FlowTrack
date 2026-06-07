using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Infrastructure;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Iam.Test.Infrastructure;

[Service]
[DomainEventSubscriber(typeof(UserCreated))]
internal sealed class OnUserCreatedDomainEventSubscriber
{
    public UserCreated? CapturedEvent { get; private set; }
    public int CalledTimes { get; private set; }

    [DomainEventListener]
    public Task OnUserCreated(UserCreated @event)
    {
        CapturedEvent = @event;
        CalledTimes++;
        return Task.CompletedTask;
    }
}

public class SignupCmdHandlerIT : IamIntegrationTestCase
{
    public SignupCmdHandlerIT(IamIntegrationFixture fixture)
        : base(fixture)
    {
        DomainEventSubscriberInformation subscriberInfo = new([
            new DomainEventSubscriberInfo(
                typeof(OnUserCreatedDomainEventSubscriber),
                typeof(OnUserCreatedDomainEventSubscriber).GetMethod("OnUserCreated")!,
                typeof(UserCreated)
            ),
        ]);

        fixture.serviceCollection.AddSingleton(subscriberInfo);
        fixture.AddScoped<SignupCmdHandler, SignupCmdHandler>();
        fixture.AddScoped<DomainEventDispatcher, DomainEventDispatcher>();
        fixture.AddScoped<IDomainEventBus, InMemoryDomainEventBus>();
        fixture.AddScoped<OnUserCreatedDomainEventSubscriber, OnUserCreatedDomainEventSubscriber>();
    }

    [Fact]
    public async Task Should_Save_User()
    {
        var userDao = _fixture.GetService<UserDao>();
        var handler = _fixture.GetService<SignupCmdHandler>();

        var user = UserMother.Inactive();
        var cmd = new SignupCmd(user.Id.ToString(), user.Email, user.Password);

        await handler.Handle(cmd);

        var savedUser = await userDao.FindById(user.Id);

        Assert.Equal(user.Id, savedUser!.Id);
        Assert.Equal(user.Email, savedUser.Email);
    }

    [Fact]
    public async Task Should_Emit_User_Created_Event()
    {
        var subscriber = _fixture.GetService<OnUserCreatedDomainEventSubscriber>();
        var handler = _fixture.GetService<SignupCmdHandler>();

        var user = UserMother.Inactive();
        var cmd = new SignupCmd(user.Id.ToString(), user.Email, user.Password);

        await handler.Handle(cmd);

        UserCreated expectedEvent = new(user.Id, user.Email, false);
        UserCreated actualEvent = subscriber.CapturedEvent!;

        Assert.NotNull(subscriber.CapturedEvent);
        Assert.Equal(expectedEvent.UserId, actualEvent.UserId);
        Assert.Equal(expectedEvent.Email, actualEvent.Email);
        Assert.Equal(expectedEvent.IsActive, actualEvent.IsActive);
    }
}
