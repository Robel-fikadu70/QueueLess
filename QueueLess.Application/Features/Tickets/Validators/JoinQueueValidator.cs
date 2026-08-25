using FluentValidation;
using QueueLess.Application.Features.Tickets.Commands;

namespace QueueLess.Application.Features.Tickets.Validators;

public class JoinQueueValidator : AbstractValidator<JoinQueueCommand>
{
    public JoinQueueValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty().WithMessage("Service ID is required to join a queue.");
    }
}