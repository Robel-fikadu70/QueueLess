using QueueLess.Domain.Common;
using System;

namespace QueueLess.Domain.Entities;

public class StaffAssignment : BaseEntity
{
    public required string StaffId { get; set; } // Maps to ASP.NET Core Identity User Id (string)
    public Guid ServiceId { get; set; }
    public int CounterNumber { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Property
    public Service? Service { get; set; }
}