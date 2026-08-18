using System.Collections.Generic;

namespace QueueLess.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email, IEnumerable<string> roles);
}