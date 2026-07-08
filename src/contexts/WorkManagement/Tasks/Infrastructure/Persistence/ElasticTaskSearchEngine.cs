using FlowTrack.Shared.Domain.Dic;
using FlowTrack.WorkManagement.Tasks.Domain;

namespace FlowTrack.WorkManagement.Tasks.Infrastructure;

[Provider(typeof(ITaskSearchEngine), Lifetime.Singleton)]
internal sealed class ElasticTaskSearchEngine : ITaskSearchEngine
{
    public Task Index(Tasky task)
    {
        throw new NotImplementedException();
    }
}
