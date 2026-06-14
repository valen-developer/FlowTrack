using FlowTrack.Shared.Domain.FilterCriterias;
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

        var expectedWorkspace = WorkspaceMother.WithNameAndOwner(DEFAULT_WORKSPACE_NAME, userId);

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

    [Fact]
    public async Task Should_Not_Save_Workspace_If_Already_Exists()
    {
        var userId = Guid.NewGuid().ToString();
        var command = new CreateDefaultWorkspaceCmd(userId);

        var filters = new Filters([
            new Filter(
                new FilterField("ownerId"),
                new FilterOperator(FilterOperators.Equals),
                new FilterValue(userId)
            ),
            new Filter(
                new FilterField("name"),
                new FilterOperator(FilterOperators.Equals),
                new FilterValue(DEFAULT_WORKSPACE_NAME)
            ),
        ]);

        var criteria = new FilterCriteria(filters, Order.None);

        var existingWorkspace = WorkspaceMother.WithNameAndOwner(DEFAULT_WORKSPACE_NAME, userId);
        _workspaceRepositoryMock
            .Setup(repo => repo.Matching(It.Is<FilterCriteria>(c => c.Equals(criteria))))
            .ReturnsAsync([existingWorkspace]);

        await _handler.Handle(command);

        _workspaceRepositoryMock.Verify(repo => repo.Save(It.IsAny<Workspace>()), Times.Never);
    }
}
