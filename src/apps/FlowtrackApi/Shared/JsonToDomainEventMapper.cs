using System.Text.Json;
using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Domain;

namespace FlowtrackApi;

[Provider(typeof(IJsonToDomainEventMapper), Lifetime.Singleton)]
public sealed class JsonToDomainEventMapper : IJsonToDomainEventMapper
{
    public DomainEvent? Map(string json)
    {
        var jsonApi = JsonSerializer.Deserialize<object>(json) as JsonElement?;
        var data = jsonApi?.GetProperty("data");
        var attributes = data?.GetProperty("attributes");
        var eventCode = data?.GetProperty("code").GetString();

        if (eventCode == UserCreated.Code)
        {
            return attributes!.Value.Deserialize<UserCreated>();
        }

        return null;
    }
}
