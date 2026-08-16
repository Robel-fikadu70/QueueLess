using QueueLess.Domain.Common;
using QueueLess.Domain.Enums;
using System.Collections.Generic;

namespace QueueLess.Domain.Entities;

public class Facility : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Location { get; set; }
    public required string OperatingHours { get; set; } // e.g., "08:00 - 17:00"
    public QueueStatus Status { get; set; } = QueueStatus.Closed;
    public bool IsDeleted { get; set; } = false;

    // Navigation Property
    public ICollection<Service> Services { get; set; } = new List<Service>();
}