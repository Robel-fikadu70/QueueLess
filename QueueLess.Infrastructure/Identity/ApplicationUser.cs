using Microsoft.AspNetCore.Identity;
using System;

namespace QueueLess.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public required string FirstName {get; set;}
    public required string LastName {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    
}