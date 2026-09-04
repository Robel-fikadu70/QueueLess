using System.Collections.Generic;

namespace QueueLess.Application.Interfaces;

public record TokenResult(string AccessToken, string RefreshToken, string JwtId);

public interface IJwtTokenGenerator
{
    TokenResult GenerateToken(string userId, string email, IEnumerable<string> roles);
}