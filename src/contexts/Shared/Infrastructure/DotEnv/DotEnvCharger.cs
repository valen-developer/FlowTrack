using dotenv.net;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure.DotEnv;

[Provider(typeof(IDotEnvCharger))]
public class DotEnvCharger : IDotEnvCharger
{
    public void Load(string[] paths)
    {
        dotenv.net.DotEnv.Load(new(envFilePaths: paths));
    }
}
