using System.Text.Json.Serialization;

namespace QueueLess.Application.DTOs.Users;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "profileType")]
[JsonDerivedType(typeof(UserProfileDto), "default")]
[JsonDerivedType(typeof(StaffProfileDto), "staff")]
public class UserProfileDto
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role {get; set; }
}