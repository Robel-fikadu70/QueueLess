using System;
using System.Threading.Tasks;

namespace QueueLess.Application.Interfaces;

public interface IQueueNotificationService
{
    // Pushes real-time status updates directly to a specific customer's ticket dashboard
    Task NotifyTicketStatusChangedAsync(string customerId, Guid ticketId, string ticketNumber, string newState);
    
    // Notifies all waiting customers in a specific service queue to recalculate their position
    Task NotifyQueuePositionChangedAsync(Guid serviceId);
}