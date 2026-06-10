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
        var attributes = jsonApi?.GetProperty("data").GetProperty("attributes");
        var eventCode = attributes?.GetProperty("code").GetString();

        return eventCode switch
        {
            "iam.user.created" => attributes!.Value.Deserialize<UserCreated>(),
            _ => null,
        };
    }
}
