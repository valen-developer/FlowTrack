using FlowTrack.WorkManagement.Tasks.Application;
using FlowTrack.WorkManagement.Tasks.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.WorkManagement.Tasks.Test.Application;

/*
* [] Should throw Task Name Too Long Exception
  [] Should throw TaskNameNull Exception
  [] Should save task in repository
  [] Should emit TaskCreated Event
*/

public class CreateTaskCmdTest
{
    private readonly CreateTaskCmdHandler _handler;

    public CreateTaskCmdTest()
    {
        var services = new ServiceCollection();

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
            Title: null!,
            Description: "Test Description",
            State: TaskStateEnum.TODO.ToString()
        );

        await Assert.ThrowsAsync<NullTaskTitle>(async () =>
        {
            await _handler.Handle(command);
        });
    }
}
