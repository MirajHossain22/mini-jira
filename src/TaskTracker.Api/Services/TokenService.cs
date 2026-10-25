using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TaskTracker.Api.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpiryMinutes { get; init; } = 60;
}

public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    public string Create(Guid userId, string email)
    {
        var jwt = options.Value;
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), new Claim(JwtRegisteredClaimNames.Email, email), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes), signingCredentials: credentials));
    }
}
