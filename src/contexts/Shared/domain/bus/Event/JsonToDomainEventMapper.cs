using System.Text.Json;

namespace FlowTrack.Shared.Domain.Bus.Event
{
    public abstract class JsonToDomainEventMapper
    {
        public abstract DomainEvent? Map(string json);

        protected string? GetCode(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var data = root.GetProperty("data");
            return data.GetProperty("code").GetString();
        }

        protected T? Serialize<T>(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var data = root.GetProperty("data");
            var attributes = data.GetProperty("attributes");
            return JsonSerializer.Deserialize<T>(attributes.GetRawText());
        }
    }
}
