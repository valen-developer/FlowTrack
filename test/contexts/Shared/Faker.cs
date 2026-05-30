namespace Shared;

public class Faker
{
    public string Email() => $"{Guid.NewGuid()}@email.com";

    public string FromRegex(string pattern) =>
        System.Text.RegularExpressions.Regex.Replace(pattern, @"\w", "x");
}
