using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure;

public class Bcrypt : IBcrypt
{
    public bool Compare(string value, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(value, hash);
    }
}
