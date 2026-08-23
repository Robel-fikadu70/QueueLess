using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Infrastructure.Services;

public class TicketExpiryBackgroundWorker(
    ITicketExpiryQueue expiryQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<TicketExpiryBackgroundWorker> logger) : BackgroundService
{
    private readonly ITicketExpiryQueue _expiryQueue = expiryQueue;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory; // Used to prevent the Captive Dependency Bug
    private readonly ILogger<TicketExpiryBackgroundWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ticket Expiration Background Worker initialized.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Dequeue tasks as they are produced by our HTTP controllers
                var task = await _expiryQueue.DequeueExpiryCheckAsync(stoppingToken);

                // Calculate the delay until the grace period threshold is hit
                var delay = task.ExpirationTime - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                // Process the expiration check securely within a dynamically allocated scope
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IQlDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<IQueueNotificationService>();


                var ticket = await dbContext.Tickets.FindAsync([task.TicketId], stoppingToken);

                if (ticket != null)
                {
                    // If the customer has not checked in (arrived) by the time the grace period expires, skip them
                    if (ticket.State == TicketState.Called && !ticket.CheckedInAt.HasValue)
                    {
                        ticket.State = TicketState.NoShow;
                        ticket.LastModifiedAt = DateTime.UtcNow;

                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogWarning("Ticket {TicketNumber} transitioned to No-Show due to grace period expiration.", ticket.TicketNumber);

                        //Real-Time Push: Notify the customer immediately that they have been skipped
                        await notificationService.NotifyTicketStatusChangedAsync(
                            ticket.CustomerId,
                            ticket.Id,
                            ticket.TicketNumber,
                            ticket.State.ToString().ToUpper()
                        );

                        //Real-Time push: Notify remaining queue candidates to shift positions
                        await notificationService.NotifyQueuePositionChangedAsync(ticket.ServiceId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore cancel exceptions during hosted shutdown sequences
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred inside the Ticket Expiration Background Worker.");
            }
        }
    }
}