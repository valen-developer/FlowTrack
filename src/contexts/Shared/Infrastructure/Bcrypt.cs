using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

[Provider(typeof(IBcrypt))]
public class Bcrypt : IBcrypt
{
    public bool Compare(string value, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(value, hash);
    }
}
