using System;

namespace QueueLess.Application.DTOs.Staff;

public class StaffMemberDto
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public Guid? AssignedServiceId { get; set; }
    public string? AssignedServiceName { get; set; }
    public int? CounterNumber { get; set; }
}