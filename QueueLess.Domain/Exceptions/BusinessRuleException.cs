namespace QueueLess.Domain.Exceptions;

public class BusinessRuleException(string message) : Exception(message)
{
}