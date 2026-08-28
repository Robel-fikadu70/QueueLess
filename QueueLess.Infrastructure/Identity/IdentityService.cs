using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.DTOs.Auth;
using QueueLess.Application.DTOs.Staff;
using QueueLess.Application.DTOs.Users;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Exceptions;

namespace QueueLess.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenGenerator tokenGenerator,
    IQlDbContext qlDbContext
) : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtTokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IQlDbContext _context = qlDbContext;

    public async Task<AuthResponseDto> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName
    )
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new BusinessRuleException("Email address is already in use.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException($"User registration failed: {errors}");
        }

        //Assign standard role
        await _userManager.AddToRoleAsync(user, "Customer");
        var roles = await _userManager.GetRolesAsync(user);

        //generate token upon successful registration
        var token = _tokenGenerator.GenerateToken(user.Id, user.Email!, roles);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Token = token,
        };
    }

    public async Task<AuthResponseDto> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new BusinessRuleException("Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenGenerator.GenerateToken(user.Id, user.Email!, roles);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Token = token,
        };
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");
        return new UserProfileDto
        {
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }

    public async Task UpdateProfileAsync(string userId, string firstName, string lastName)
    {
        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.FirstName = firstName;
        user.LastName = lastName;

        await _userManager.UpdateAsync(user);
    }

    public async Task<string> RegisterStaffAsync(
        string email,
        string password,
        string firstName,
        string lastName
    )
    {
        var existingUser = _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email address is already in use.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Staff Registration failed: {error}");
        }

        await _userManager.AddToRoleAsync(user, "Staff");
        return user.Id;
    }

    public async Task<IEnumerable<StaffMemberDto>> GetStaffUsersAsync()
    {
        //fetch all user assigned to the staff role
        var staffUsers = await _userManager.GetUsersInRoleAsync("Staff");

        var assignments = await _context
            .StaffAssignments.Include(sa => sa.Service)
            .Where(sa => sa.IsActive)
            .ToListAsync();
        return staffUsers.Select(user =>
        {
            var assignment = assignments.FirstOrDefault(a => a.StaffId == user.Id);
            return new StaffMemberDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AssignedServiceId = assignment?.ServiceId,
                AssignedServiceName = assignment?.Service?.Name,
                CounterNumber = assignment?.CounterNumber,
            };
        });
    }
}
