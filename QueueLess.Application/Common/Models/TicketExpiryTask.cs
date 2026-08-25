namespace QueueLess.Application.Common.Models;

public record TicketExpiryTask(Guid TicketId, DateTime ExpirationTime);