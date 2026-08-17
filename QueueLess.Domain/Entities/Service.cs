using QueueLess.Domain.Common;
using System;
using System.Collections.Generic;

namespace QueueLess.Domain.Entities;

public class Service : BaseEntity
{
    public Guid FacilityId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;


    // Navigation Properties
    public Facility? Facility { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<StaffAssignment> StaffAssignments { get; set; } = new List<StaffAssignment>();
}