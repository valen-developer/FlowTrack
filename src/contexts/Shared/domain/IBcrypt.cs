namespace FlowTrack.Shared.Domain;

public interface IBcrypt
{
    bool Compare(string value, string hash);
    string Hash(string v);
}
