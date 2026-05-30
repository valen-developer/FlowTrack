namespace Shared.domain;

public interface IBcrypt
{
    bool Compare(string value, string hash);
}
