using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure
{
    [Provider(typeof(IBcrypt))]
    public class Bcrypt : IBcrypt
    {
        public bool Compare(string value, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(value, hash);
            }
            catch
            {
                return false;
            }
        }

        public string Hash(string v)
        {
            return BCrypt.Net.BCrypt.HashPassword(v);
        }
    }
}
