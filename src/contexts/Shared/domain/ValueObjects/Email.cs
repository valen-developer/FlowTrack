using System.Text.RegularExpressions;
using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Shared.Domain.ValueObjects;

public record Email : ValueObject<string>
{
    public static readonly Regex EmailRegex = new(
        @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
        RegexOptions.Compiled
    );

    public Email(string value)
        : base(value)
    {
        EnsureEmail(value);
    }

    public static void EnsureEmail(string value)
    {
        if (!EmailRegex.IsMatch(value))
        {
            throw new InvalidEmail(value);
        }
    }
}
