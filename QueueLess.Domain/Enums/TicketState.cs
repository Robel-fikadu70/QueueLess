namespace QueueLess.Domain.Enums;

public enum TicketState
{
    Waiting,
    Called,
    CheckedIn,
    Serving,
    Completed,
    Cancelled,
    NoShow
}