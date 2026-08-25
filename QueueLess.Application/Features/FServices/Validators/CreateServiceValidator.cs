using FluentValidation;
using QueueLess.Application.Features.FServices.Commands;

namespace QueueLess.Application.Features.FServices.Validators;

public class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EstimatedDurationMinutes).GreaterThan(0).WithMessage("Estimated duration must be positive.");
    }
}