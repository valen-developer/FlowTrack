namespace FlowTrack.Shared.Test;

public class Faker
{
    public bool boolean() => new Random().Next(0, 2) == 1;

    public string Email() => $"{Guid.NewGuid()}@email.com";

    public string FromRegex(string pattern) =>
        System.Text.RegularExpressions.Regex.Replace(pattern, @"\w", "x");
}
