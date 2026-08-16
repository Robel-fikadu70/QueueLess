using QueueLess.Domain.Common;
using QueueLess.Domain.Enums;
using System;

namespace QueueLess.Domain.Entities;

public class Ticket : BaseEntity
{
    public Guid ServiceId { get; set; }
    public required string TicketNumber { get; set; } // e.g., "LAB-127"
    public int SequenceNumber { get; set; } // Daily counter resetting per service
    public TicketState State { get; set; } = TicketState.Waiting;
    
    // User Identifiers
    public required string CustomerId { get; set; } // Maps to ASP.NET Core Identity User Id (string)
    public string? ServedByStaffId { get; set; } // Maps to Staff Identity User Id (string)

    // Wait-time / Grace-period tracking timestamps
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? ServedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation Property
    public Service? Service { get; set; }
}