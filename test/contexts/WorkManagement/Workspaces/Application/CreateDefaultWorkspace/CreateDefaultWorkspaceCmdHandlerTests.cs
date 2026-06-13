using FlowTrack.WorkManagement.Workspaces.Application;
using FlowTrack.WorkManagement.Workspaces.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.WorkManagement.Workspaces.Test.Application;

public class CreateDefaultWorkspaceCmdHandlerTests
{
    private readonly string DEFAULT_WORKSPACE_NAME = "Default";

    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock = new();
    private readonly CreateDefaultWorkspaceCmdHandler _handler;

    public CreateDefaultWorkspaceCmdHandlerTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_workspaceRepositoryMock.Object);
        services.AddSingleton<CreateDefaultWorkspaceCmdHandler>();

        var serviceProvider = services.BuildServiceProvider();
        _handler = serviceProvider.GetRequiredService<CreateDefaultWorkspaceCmdHandler>();
    }

    [Fact]
    public async Task Should_Save_Default_Workspace_On_Repository()
    {
        var userId = Guid.NewGuid().ToString();
        var command = new CreateDefaultWorkspaceCmd(userId);

        var expectedWorkspace = new Workspace(
            Id: new WorkspaceId(Guid.NewGuid().ToString()),
            Name: new WorkspaceName(DEFAULT_WORKSPACE_NAME),
            OwnerId: new WorkspaceOwnerId(userId)
        );

        await _handler.Handle(command);

        _workspaceRepositoryMock.Verify(
            repo =>
                repo.Save(
                    It.Is<Workspace>(w =>
                        w.OwnerId == expectedWorkspace.OwnerId && w.Name == expectedWorkspace.Name
                    )
                ),
            Times.Once
        );
    }
}
