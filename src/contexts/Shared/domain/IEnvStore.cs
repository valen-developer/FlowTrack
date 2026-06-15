namespace FlowTrack.Shared.Domain
{
    public interface IEnvStore
    {
        string? Get(string key);
    }
}
