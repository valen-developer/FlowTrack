using System.Text.RegularExpressions;

namespace FlowTrack.Shared.Domain;

public class Email
{
    public static readonly Regex EmailRegex = new(
        @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
        RegexOptions.Compiled
    );
    public string Value { get; }

    public Email(string value)
    {
        EnsureEmail(value);
        Value = value;
    }

    public static void EnsureEmail(string value)
    {
        if (!EmailRegex.IsMatch(value))
        {
            throw new InvalidEmail(value);
        }
    }
}
