namespace QueueLess.Application.DTOs.Users;

public class UserProfileDto
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role {get; set; }
}