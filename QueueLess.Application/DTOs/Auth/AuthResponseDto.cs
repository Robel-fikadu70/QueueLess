namespace QueueLess.Application.DTOs.Auth;

public class AuthResponseDto
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Token {get; set;}
}