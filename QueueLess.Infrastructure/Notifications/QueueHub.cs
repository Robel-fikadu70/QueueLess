using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace QueueLess.Infrastructure.Notifications;

[Authorize] // Requires authenticated connection sessions
public class QueueHub : Hub
{
    // Clients call this to subscribe to updates specifically for their ticket
    public async Task JoinTicketGroup(string ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Ticket-{ticketId}");
    }

    // Clients call this to subscribe to updates for an entire service queue (to listen for position updates)
    public async Task JoinServiceGroup(string serviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Service-{serviceId}");
    }
}