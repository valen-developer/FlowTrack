using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Tasks.Application;
using FlowTrack.WorkManagement.Tasks.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.WorkManagement.Tasks.Test.Application;

public class IndexTaskOnTaskCreatedTest
{
    private readonly Mock<ITaskRepository> _repositoryMock = new();
    private readonly Mock<ITaskSearchEngine> _searchEngineMock = new();

    private readonly IndexTaskOnTaskCreated _handler;

    public IndexTaskOnTaskCreatedTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_searchEngineMock.Object);
        services.AddSingleton(_repositoryMock.Object);

        services.AddScoped<IndexTaskOnTaskCreated>();

        var serviceProvider = services.BuildServiceProvider();
        _handler = serviceProvider.GetRequiredService<IndexTaskOnTaskCreated>();
    }

    [Fact]
    public async Task Should_Index_In_Search_Engine()
    {
        var task = TaskMother.WithId(Guid.NewGuid().ToString());
        var @event = new TaskCreated(
            Id: task.Id.Value,
            OwnerId: task.OwnerId.Value,
            WorkspaceId: task.WorkspaceId.Value,
            Title: task.Title.Value,
            Description: task.Description.Value,
            State: task.State.Value.ToString()
        );

        var filters = new Filters([new(new("Id"), new(FilterOperators.Equals), new(@event.Id))]);
        var criteria = new FilterCriteria(filters, Order.None);

        _repositoryMock
            .Setup(r => r.MatchingOne(It.Is<FilterCriteria>(c => c.Equals(criteria))))
            .ReturnsAsync(task);

        await _handler.On(@event);

        _searchEngineMock.Verify(s => s.Index(task), Times.Once);
    }
}
