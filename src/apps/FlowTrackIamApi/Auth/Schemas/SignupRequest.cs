namespace FlowTrackIamApi.Auth.Schemas;

public sealed class SignupRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
