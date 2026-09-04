using System;

namespace QueueLess.Application.DTOs.Users;

public class StaffProfileDto : UserProfileDto
{
    public Guid? AssignedServiceId { get; set; }
    public string? AssignedServiceName { get; set; }
    public int? CounterNumber { get; set; }
}