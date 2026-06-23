using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Tasks.Domain;

namespace FlowTrack.WorkManagement.Tasks.Application;

[Service]
[DomainEventSubscriber(typeof(TaskCreated))]
internal sealed class IndexTaskOnTaskCreated(
    ITaskRepository repository,
    ITaskSearchEngine searchEngine
)
{
    [DomainEventListener]
    public async Task On(TaskCreated @event)
    {
        Filters filters = new([new(new("Id"), new(FilterOperators.Equals), new(@event.Id))]);
        FilterCriteria criteria = new(filters, Order.None);

        var task = await repository.MatchingOne(criteria);

        if (task is not null)
        {
            await searchEngine.Index(task);
        }
    }
}
