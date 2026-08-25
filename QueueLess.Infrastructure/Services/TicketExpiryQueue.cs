using System.Threading.Channels;
using QueueLess.Application.Common.Models;
using QueueLess.Application.Interfaces;

namespace QueueLess.Infrastructure.Services;

public class TicketExpiryQueue : ITicketExpiryQueue
{
    private readonly Channel<TicketExpiryTask> _queue;

    public TicketExpiryQueue()
    {
        _queue = Channel.CreateUnbounded<TicketExpiryTask>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async ValueTask QueueExpiryCheckAsync(TicketExpiryTask task, CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(task, cancellationToken);
    }

    public async ValueTask<TicketExpiryTask> DequeueExpiryCheckAsync(CancellationToken cancellationToken = default)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}