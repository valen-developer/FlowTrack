using FlowTrack.Shared.Domain.Iam.Users;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Iam.Test.Auth.Infrastructure
{
    [Service]
    [DomainEventSubscriber(typeof(UserCreated))]
    internal sealed class OnUserCreatedDomainEventSubscriber
    {
        public UserCreated? CapturedEvent { get; private set; }
        public int CalledTimes { get; private set; }

        [DomainEventListener]
        public Task On(UserCreated @event)
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
                    typeof(OnUserCreatedDomainEventSubscriber).GetMethod("On")!,
                    typeof(UserCreated)
                ),
            ]);

            fixture.serviceCollection.AddSingleton(subscriberInfo);
            fixture.AddScoped<EventBus, EventBus>();
            fixture.AddScoped<SignupCmdHandler, SignupCmdHandler>();
            fixture.AddScoped<DomainEventDispatcher, DomainEventDispatcher>();
            fixture.AddScoped<IDomainEventBus, InMemoryDomainEventBus>();
            fixture.AddScoped<IExternalEventBus, InMemoryExternalEventBus>();
            fixture.AddScoped<
                OnUserCreatedDomainEventSubscriber,
                OnUserCreatedDomainEventSubscriber
            >();
        }

        [Fact]
        public async Task Should_Save_User()
        {
            var userDao = _fixture.GetService<UserDao>();
            var handler = _fixture.GetService<SignupCmdHandler>();

            var user = UserMother.Inactive();
            var cmd = new SignupCmd(user.Id.Value, user.Email.Value, user.Password.Value);

            await handler.Handle(cmd);

            var savedUser = await userDao.FindById(user.Id.Value);

            Assert.Equal(user.Id.Value, savedUser!.Id.ToString());
            Assert.Equal(user.Email.Value, savedUser.Email);
        }

        [Fact]
        public async Task Should_Emit_User_Created_Event()
        {
            var subscriber = _fixture.GetService<OnUserCreatedDomainEventSubscriber>();
            var handler = _fixture.GetService<SignupCmdHandler>();

            var user = UserMother.Inactive();
            var cmd = new SignupCmd(user.Id.Value, user.Email.Value, user.Password.Value);

            await handler.Handle(cmd);

            UserCreated expectedEvent = new(
                UserId: user.Id.Value,
                Email: user.Email.Value,
                IsActive: user.IsActive
            );
            UserCreated actualEvent = subscriber.CapturedEvent!;

            Assert.NotNull(subscriber.CapturedEvent);
            Assert.Equal(expectedEvent.UserId, actualEvent.UserId);
            Assert.Equal(expectedEvent.Email, actualEvent.Email);
        }
    }
}
