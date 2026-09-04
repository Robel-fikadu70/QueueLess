using QueueLess.Domain.Enums;

namespace QueueLess.Application.DTOs.Tickets;

public class TicketDashboardDto
{
    public required string FacilityName { get; set; }
    public required string ServiceName { get; set; }
    public required string TicketNumber { get; set; }
    public int PeopleAhead { get; set; }
    public required string CurrentTicketBeingServed { get; set; }
    public required string EstimatedWaitRange { get; set; }
    public required string QueueStatus { get; set; } // OPEN, PAUSED, CLOSED
    public required TicketState CheckInStatus { get; set; } // Checked In, Pending
}