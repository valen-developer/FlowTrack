namespace Shared.Domain;

public record JWTPayload
{
    public IReadOnlyDictionary<string, string> Claims { get; }

    public JWTPayload(IReadOnlyDictionary<string, string> claims)
    {
        Claims = claims;
    }
}

public record JWTOptions
{
    public string Secret { get; }
    public int ExpirationMinutes { get; }

    public JWTOptions(string secret, int expirationMinutes)
    {
        Secret = secret;
        ExpirationMinutes = expirationMinutes;
    }
}

public interface IJWTService
{
    string Generate(JWTPayload payload, JWTOptions options);
}
