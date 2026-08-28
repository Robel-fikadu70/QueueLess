using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QueueLess.Application.Interfaces;

namespace QueueLess.Infrastructure.Identity;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration = configuration;

    public TokenResult GenerateToken(string userId, string email, IEnumerable<string> roles)
    {
        var jwtId = Guid.NewGuid().ToString();

        // Set up the list of claims (user identity statements)
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, jwtId),
        };

        //Add user roles as claims for role-based authorization
        foreach(var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        //Retrive security key from configuration
        var keyString = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Short-Lived Access Token: Valid for only 15 minutes
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), 
            signingCredentials: creds
        );
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        
        // Long-Lived Cryptographically Secure Refresh Token
        var refreshToken = GenerateSecureRandomString();

        return new TokenResult(accessToken, refreshToken, jwtId);
    }

    private static string GenerateSecureRandomString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}