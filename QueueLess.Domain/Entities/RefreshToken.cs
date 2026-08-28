using QueueLess.Domain.Common;

namespace QueueLess.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    public required string JwtId { get; set; } // Links to the JTI of the issued Access Token to detect theft
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public DateTime ExpiryDate { get; set; }
    public required string UserId { get; set; } // Reference to AspNetUsers table
}