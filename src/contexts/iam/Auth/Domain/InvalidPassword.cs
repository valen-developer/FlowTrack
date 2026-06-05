using System.Text.RegularExpressions;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public class InvalidPassword()
    : InvalidException(
        "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter and one digit.",
        "exception.iam.auth.password.invalid"
    )
{
    // public static readonly Regex Regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
    public static readonly Regex Regex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        RegexOptions.Compiled
    );
}
