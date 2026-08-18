using FluentValidation;
using QueueLess.Application.Features.Facilities.Commands;

namespace QueueLess.Application.Features.Facilities.Validators;

public class CreateFacilityValidator : AbstractValidator<CreateFacilityCommand>
{
    public CreateFacilityValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(250);
        RuleFor(x => x.OperatingHours).NotEmpty().MaximumLength(50);
    }
}