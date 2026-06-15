namespace FlowTrack.Iam.Auth.Domain
{
    internal class Password(string value)
    {
        public string Value { get; } = value;

        public static Password EnsurePassword(string value)
        {
            var regex = InvalidPassword.Regex;
            if (!regex.IsMatch(value))
            {
                throw new InvalidPassword();
            }

            return new Password(value);
        }
    }
}
