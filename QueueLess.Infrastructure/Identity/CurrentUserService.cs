using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QueueLess.Application.Interfaces;

namespace QueueLess.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    //safely reads the Sub claim from the decrypted JWT payload inside the cookie
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}