using FlowTrack.Shared.Domain.Iam.Users;

namespace FlowTrack.Iam.Shared.Infrastructure
{
    [Provider(typeof(JsonToDomainEventMapper), Lifetime.Singleton)]
    public sealed class JsonToDomainEventMapperIam : JsonToDomainEventMapper
    {
        public override DomainEvent? Map(string json)
        {
            var code = GetCode(json);
            return code == UserCreated.Code ? Serialize<UserCreated>(json) : null;
        }
    }
}
