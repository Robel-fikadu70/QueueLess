using System.Threading.Tasks;
using QueueLess.Application.DTOs.Auth;
using QueueLess.Application.DTOs.Staff;
using QueueLess.Application.DTOs.Users;

namespace QueueLess.Application.Interfaces;

public interface IIdentityService
{
    Task<AuthResponseDto> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<AuthResponseDto> LoginAsync(string email, string password);
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task UpdateProfileAsync(string userId, string firstName, string lastName);
    Task<string> RegisterStaffAsync(string email, string password, string firstName, string lastName);
    Task<IEnumerable<StaffMemberDto>> GetStaffUsersAsync();
}