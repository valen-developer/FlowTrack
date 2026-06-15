namespace FlowTrack.Shared.Domain
{
    public interface IDotEnvCharger
    {
        void Load(string[] paths);
    }
}
