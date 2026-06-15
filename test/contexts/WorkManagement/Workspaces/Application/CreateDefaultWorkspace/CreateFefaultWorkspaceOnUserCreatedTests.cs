using FlowTrack.Shared.Domain.Bus.Command;
using FlowTrack.Shared.Domain.Iam.Users;
using FlowTrack.WorkManagement.Workspaces.Application;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.WorkManagement.Workspaces.Test.Application
{
    public class CreateDefaultWorkspaceOnUserCreatedTests
    {
        private readonly Mock<ICommandBus> _commandBusMock = new();
        private readonly CreateDefaultWorkspaceOnUserCreated _subscriber;

        public CreateDefaultWorkspaceOnUserCreatedTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_commandBusMock.Object);

            services.AddScoped<CreateDefaultWorkspaceOnUserCreated>();

            var serviceProvider = services.BuildServiceProvider();
            _subscriber = serviceProvider.GetRequiredService<CreateDefaultWorkspaceOnUserCreated>();
        }

        [Fact]
        public async Task Should_Call_Command_Bus_With_Command()
        {
            var userId = Guid.NewGuid().ToString();
            var email = "email@email.com";
            var isActive = false;

            var userCreatedEvent = new UserCreated(
                UserId: userId,
                Email: email,
                IsActive: isActive
            );
            var expectedCommand = new CreateDefaultWorkspaceCmd(userId);

            await _subscriber.On(userCreatedEvent);

            _commandBusMock.Verify(
                bus =>
                    bus.Dispatch(
                        It.Is<CreateDefaultWorkspaceCmd>(cmd =>
                            cmd.UserId == expectedCommand.UserId
                        )
                    ),
                Times.Once
            );
        }
    }
}
