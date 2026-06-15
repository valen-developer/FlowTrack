namespace FlowtrackApi.Iam.Schemas
{
    public sealed class SigninRequestDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
