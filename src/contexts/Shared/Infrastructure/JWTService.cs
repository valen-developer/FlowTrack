namespace FlowTrack.Shared.Infrastructure;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlowTrack.Shared.Domain;
using Microsoft.IdentityModel.Tokens;

public class JWTService : IJWTService
{
    public string Generate(JWTPayload payload, JWTOptions options)
    {
        var claims = payload.Claims.Select(x => new Claim(x.Key, x.Value)).ToList();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(options.ExpirationMinutes);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
