namespace QueueLess.Domain.Enums;

public enum TicketState
{
    Waiting,
    Called,
    Serving,
    Completed,
    Cancelled,
    NoShow
}