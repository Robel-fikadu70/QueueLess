namespace QueueLess.Application.DTOs.Tickets;

public class TicketHistoryDto
{
    public Guid TicketId { get; set; }
    public required string TicketNumber { get; set; }
    public required string ServiceName { get; set; }
    public required string FacilityName { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}