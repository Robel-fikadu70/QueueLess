using System.Threading.Tasks;
using QueueLess.Application.DTOs.Auth;

namespace QueueLess.Application.Interfaces;

public interface IIdentityService
{
    Task<AuthResponseDto> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<AuthResponseDto> LoginAsync(string email, string password);
}