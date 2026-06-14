using FlowTrack.Shared.Domain.Bus.Event;
using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.Iam.Users;

namespace FlowTrack.WorkManagement.Shared.Infrastructure;

[Provider(typeof(JsonToDomainEventMapper), Lifetime.Singleton)]
public sealed class JsonToDomainEventMapperWorkmanagement : JsonToDomainEventMapper
{
    public override DomainEvent? Map(string json)
    {
        var code = GetCode(json);
        if (code == UserCreated.Code)
        {
            return Serialize<UserCreated>(json);
        }

        return null;
    }
}
