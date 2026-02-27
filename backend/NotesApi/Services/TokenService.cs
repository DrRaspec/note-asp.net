using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NotesApi.Models;

namespace NotesApi.Services;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public string CreateToken(User user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var minutes) ? minutes : 120;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Token, DateTime ExpiresAt) CreateRefreshToken()
    {
        var jwtSection = configuration.GetSection("Jwt");
        var refreshDays = int.TryParse(jwtSection["RefreshExpiresDays"], out var days) ? days : 7;
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (token, DateTime.UtcNow.AddDays(refreshDays));
    }
}
