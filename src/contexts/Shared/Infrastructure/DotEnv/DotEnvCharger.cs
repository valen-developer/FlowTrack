using dotenv.net;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

[Provider(typeof(IDotEnvCharger))]
public class DotEnvCharger : IDotEnvCharger
{
    public void Load(string[] paths)
    {
        DotEnv.Load(new(envFilePaths: paths));
    }
}
