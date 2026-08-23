using QueueLess.Application.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Interfaces;

public interface ITicketExpiryQueue
{
    ValueTask QueueExpiryCheckAsync(TicketExpiryTask task, CancellationToken cancellationToken = default);
    ValueTask<TicketExpiryTask> DequeueExpiryCheckAsync(CancellationToken cancellationToken = default);
}