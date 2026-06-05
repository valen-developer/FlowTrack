using System.Text.RegularExpressions;
using Fare;

namespace FlowTrack.Shared.Test;

public class Faker
{
    public bool boolean() => new Random().Next(0, 2) == 1;

    public string Email() => $"{Guid.NewGuid()}@email.com";

    public string FromRegex(Regex pattern)
    {
        var xeger = new Xeger(pattern.ToString());
        return xeger.Generate();
    }
}
