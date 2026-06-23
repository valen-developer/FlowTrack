using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.WorkManagement.Tasks.Application;
using FlowTrack.WorkManagement.Tasks.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.WorkManagement.Tasks.Test.Application;

/*
* [] Should throw Task Name Too Long Exception
  [] Should throw TaskNameNull Exception
  [] Should throw InvalidTaskState Exception
  [] Should save task in repository
  [] Should emit TaskCreated Event
*/

public class CreateTaskCmdTest
{
    private readonly Mock<ITaskRepository> _repositoryMock = new();
    private readonly Mock<IDomainEventBus> _internalEventBusMock = new();
    private readonly Mock<IExternalEventBus> _externalEventBusMock = new();
    private readonly CreateTaskCmdHandler _handler;

    public CreateTaskCmdTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_internalEventBusMock.Object);
        services.AddSingleton(_externalEventBusMock.Object);
        services.AddSingleton(_repositoryMock.Object);
        services.AddSingleton<EventBus>();
        services.AddScoped<CreateTaskCmdHandler>();

        var serviceProvider = services.BuildServiceProvider();
        _handler = serviceProvider.GetRequiredService<CreateTaskCmdHandler>();
    }

    [Fact]
    public async Task Should_Throw_Task_Title_Too_Long_Exception()
    {
        var maxNameLength = 255;

        var command = new CreateTaskCmd(
            Id: Guid.NewGuid().ToString(),
            OwnerId: Guid.NewGuid().ToString(),
            WorkspaceId: Guid.NewGuid().ToString(),
            Title: new string('A', maxNameLength + 1),
            Description: "Test Description",
            State: TaskStateEnum.TODO.ToString()
        );

        await Assert.ThrowsAsync<TaskTitleTooLong>(async () =>
        {
            await _handler.Handle(command);
        });
    }

    [Fact]
    public async Task Should_Throw_NullTaskTitle_Exception()
    {
        var command = new CreateTaskCmd(
            Id: Guid.NewGuid().ToString(),
            OwnerId: Guid.NewGuid().ToString(),
            WorkspaceId: Guid.NewGuid().ToString(),
            Title: null!,
            Description: "Test Description",
            State: TaskStateEnum.TODO.ToString()
        );

        await Assert.ThrowsAsync<NullTaskTitle>(async () =>
        {
            await _handler.Handle(command);
        });
    }

    [Fact]
    public async Task Should_Throw_InvalidTaskState_Exception()
    {
        var command = new CreateTaskCmd(
            Id: Guid.NewGuid().ToString(),
            OwnerId: Guid.NewGuid().ToString(),
            WorkspaceId: Guid.NewGuid().ToString(),
            Title: "Test Task",
            Description: "Test Description",
            State: "INVALID_STATE"
        );

        await Assert.ThrowsAsync<InvalidTaskState>(async () =>
        {
            await _handler.Handle(command);
        });
    }

    [Theory]
    [InlineData("TODO")]
    [InlineData("IN_PROGRESS")]
    [InlineData("DONE")]
    public async Task Should_Not_Throw_InvalidTaskState_Exception(string state)
    {
        var command = new CreateTaskCmd(
            Id: Guid.NewGuid().ToString(),
            OwnerId: Guid.NewGuid().ToString(),
            WorkspaceId: Guid.NewGuid().ToString(),
            Title: "Test Task",
            Description: "Test Description",
            State: state
        );

        var exception = await Record.ExceptionAsync(async () =>
        {
            await _handler.Handle(command);
        });

        Assert.Null(exception);
    }

    [Fact]
    public async Task Should_Save_Task_In_Repository()
    {
        var command = new CreateTaskCmd(
            Id: Guid.NewGuid().ToString(),
            OwnerId: Guid.NewGuid().ToString(),
            WorkspaceId: Guid.NewGuid().ToString(),
            Title: "Test Task",
            Description: "Test Description",
            State: TaskStateEnum.TODO.ToString()
        );

        await _handler.Handle(command);

        _repositoryMock.Verify(
            repo =>
                repo.Save(
                    It.Is<Tasky>(task =>
                        task.Title.Value == command.Title
                        && task.Description.Value == command.Description
                        && task.State.Value.ToString() == command.State
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_Emit_TaskCreated_Event()
    {
        IEnumerable<DomainEvent> capturedEvents = null!;

        _internalEventBusMock
            .Setup(bus => bus.Publish(It.IsAny<IEnumerable<DomainEvent>>()))
            .Callback<IEnumerable<DomainEvent>>(events => capturedEvents = events);

        var command = new CreateTaskCmd(
            Id: Guid.NewGuid().ToString(),
            OwnerId: Guid.NewGuid().ToString(),
            WorkspaceId: Guid.NewGuid().ToString(),
            Title: "Test Task",
            Description: "Test Description",
            State: TaskStateEnum.TODO.ToString()
        );

        await _handler.Handle(command);

        TaskCreated expectedEvent = new(
            Id: command.Id,
            OwnerId: command.OwnerId,
            WorkspaceId: command.WorkspaceId,
            Title: command.Title,
            Description: command.Description,
            State: command.State
        );

        await WaitForAsync(async () =>
        {
            return capturedEvents != null;
        });

        Assert.NotNull(capturedEvents);
        Assert.Equal(expectedEvent.Id, ((TaskCreated)capturedEvents.First()).Id);
    }
}
