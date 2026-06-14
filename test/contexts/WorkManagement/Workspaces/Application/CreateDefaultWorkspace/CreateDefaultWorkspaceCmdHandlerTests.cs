using FlowTrack.Shared.Domain.Bus.Event;
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
    private readonly Mock<IDomainEventBus> _domainEventBusMock = new();
    private readonly Mock<IExternalEventBus> _externalEventBusMock = new();
    private readonly CreateDefaultWorkspaceCmdHandler _handler;

    public CreateDefaultWorkspaceCmdHandlerTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_workspaceRepositoryMock.Object);
        services.AddSingleton(_domainEventBusMock.Object);
        services.AddSingleton(_externalEventBusMock.Object);
        services.AddSingleton<EventBus>();

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

    [Fact]
    public async Task Should_Emit_WorkspaceCreated_Event()
    {
        DomainEvent? capturedEvent = null;
        Workspace? capturedWorkspace = null;

        _domainEventBusMock
            .Setup(bus => bus.Publish(It.IsAny<IEnumerable<DomainEvent>>()))
            .Callback<IEnumerable<DomainEvent>>(events =>
                capturedEvent = events.FirstOrDefault(e => e is WorkspaceCreated)
            )
            .Returns(Task.CompletedTask);

        _workspaceRepositoryMock
            .Setup(repo => repo.Save(It.IsAny<Workspace>()))
            .Callback<Workspace>(workspace => capturedWorkspace = workspace)
            .Returns(Task.CompletedTask);

        var userId = Guid.NewGuid().ToString();
        var command = new CreateDefaultWorkspaceCmd(userId);

        _workspaceRepositoryMock
            .Setup(repo => repo.Matching(It.IsAny<FilterCriteria>()))
            .ReturnsAsync([]);

        await _handler.Handle(command);

        var expectedEvent = new WorkspaceCreated(
            Id: capturedWorkspace!.Id.Value,
            Name: capturedWorkspace!.Name.Value,
            OwnerId: capturedWorkspace!.OwnerId.Value
        );

        _domainEventBusMock.Verify(
            bus => bus.Publish(It.IsAny<IEnumerable<DomainEvent>>()),
            Times.Once
        );

        WorkspaceCreated workspaceCreatedEvent = (WorkspaceCreated)capturedEvent!;
        Assert.NotNull(workspaceCreatedEvent);
        Assert.IsType<WorkspaceCreated>(workspaceCreatedEvent);
        Assert.Equal(expectedEvent.Id, workspaceCreatedEvent.Id);
        Assert.Equal(expectedEvent.Name, workspaceCreatedEvent.Name);
        Assert.Equal(expectedEvent.OwnerId, workspaceCreatedEvent.OwnerId);
    }
}
