using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.DTOs.Auth;
using QueueLess.Application.DTOs.Staff;
using QueueLess.Application.DTOs.Users;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;
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

    public async Task<TokenResult> RegisterAsync(string email, string password, string firstName, string lastName)
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
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException($"User registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        var roles = await _userManager.GetRolesAsync(user);

        var tokenResult = _tokenGenerator.GenerateToken(user.Id, user.Email!, roles);

        // Persist the Refresh Token in our database tracking table
        await SaveRefreshTokenAsync(tokenResult.RefreshToken, tokenResult.JwtId, user.Id);

        return tokenResult;
    }
    public async Task<TokenResult> LoginAsync(string email, string password)
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
        var tokenResult = _tokenGenerator.GenerateToken(user.Id, user.Email!, roles);

        await SaveRefreshTokenAsync(tokenResult.RefreshToken, tokenResult.JwtId, user.Id);

        return tokenResult;
    }

    public async Task<TokenResult> RefreshTokenAsync(string token)
    {
        // 1. Fetch token details
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (storedToken == null)
        {
            throw new BusinessRuleException("Invalid refresh token.");
        }

        // 2. TOKEN THEFT DETECTION (REUSE DETECTION)
        if (storedToken.IsUsed)
        {
            // Theft suspected: Invalidate all active tokens for this User ID
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var t in activeTokens)
            {
                t.IsRevoked = true;
                t.LastModifiedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            throw new BusinessRuleException("Token reuse detected. All sessions invalidated for security.");
        }

        if (storedToken.IsRevoked || DateTime.UtcNow > storedToken.ExpiryDate)
        {
            throw new BusinessRuleException("Refresh token has expired or has been revoked.");
        }

        // 3. Mark the current token as used
        storedToken.IsUsed = true;
        storedToken.LastModifiedAt = DateTime.UtcNow;

        // 4. Generate a rotated token pair
        var user = await _userManager.FindByIdAsync(storedToken.UserId)
            ?? throw new BusinessRuleException("User associated with token does not exist.");

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResult = _tokenGenerator.GenerateToken(user.Id, user.Email!, roles);

        await SaveRefreshTokenAsync(tokenResult.RefreshToken, tokenResult.JwtId, user.Id);
        await _context.SaveChangesAsync();

        return tokenResult;
    }
    private async Task SaveRefreshTokenAsync(string token, string jwtId, string userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            JwtId = jwtId,
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(7) // Valid for 7 days
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }
    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new BusinessRuleException("User not found.");

        var role = await _userManager.GetRolesAsync(user);
        return new UserProfileDto
        {
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role[0]
        };
    }

    public async Task UpdateProfileAsync(string userId, string firstName, string lastName)
    {
        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new BusinessRuleException("User not found.");

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
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException($"Staff Registration failed: {error}");
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
