namespace FlowTrack.Shared.Infrastructure;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlowTrack.Shared.Domain;
using Microsoft.IdentityModel.Tokens;

[Provider(typeof(IJWTService))]
public class JWTService(IDateTimeProvider datetimeProvider) : IJWTService
{
    public JWTPayload? Decode(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        if (jwtToken == null)
        {
            return null;
        }

        var claims = jwtToken.Claims.ToDictionary(x => x.Type, x => x.Value);

        return new JWTPayload(claims);
    }

    public string Generate(JWTPayload payload, JWTOptions options)
    {
        var claims = payload.Claims.Select(x => new Claim(x.Key, x.Value)).ToList();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = datetimeProvider.Now.AddMinutes(options.ExpirationMinutes);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool Verify(string token, string secret)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ValidateIssuer = false,
                    ValidateAudience = false,

                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,
                },
                out SecurityToken validatedToken
            );

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return false;
        }
    }
}
