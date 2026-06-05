namespace FlowTrack.Shared.Domain;

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
    // Decode which have Claims
    JWTPayload? Decode(string token);
    string Generate(JWTPayload payload, JWTOptions options);
    bool Verify(string token, string secret);
}
