using Microsoft.AspNetCore.SignalR;
using QueueLess.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace QueueLess.Infrastructure.Notifications;

public class QueueNotificationService(IHubContext<QueueHub> hubContext) : IQueueNotificationService
{
    private readonly IHubContext<QueueHub> _hubContext = hubContext;

    public async Task NotifyTicketStatusChangedAsync(string customerId, Guid ticketId, string ticketNumber, string newState)
    {
        // Pushes the message to the group associated with the specific ticket
        await _hubContext.Clients.Group($"Ticket-{ticketId}")
            .SendAsync("ReceiveStatusUpdate", new 
            { 
                TicketId = ticketId, 
                TicketNumber = ticketNumber, 
                State = newState 
            });
    }

    public async Task NotifyQueuePositionChangedAsync(Guid serviceId)
    {
        // Forces all clients watching this service queue to refresh their current positions
        await _hubContext.Clients.Group($"Service-{serviceId}")
            .SendAsync("QueuePositionChanged", new { ServiceId = serviceId });
    }
}