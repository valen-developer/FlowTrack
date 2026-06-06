using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

[Provider(typeof(IEnvStore))]
public class EnvStore : IEnvStore
{
    public string? Get(string key) => Environment.GetEnvironmentVariable(key);
}
