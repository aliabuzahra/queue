using FluentValidation;

namespace VirtualQueue.Application.Validators;

public class CreateQueueCommandValidator : AbstractValidator<Commands.Queues.CreateQueueCommand>
{
    public CreateQueueCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.MaxConcurrentUsers)
            .GreaterThan(0).WithMessage("MaxConcurrentUsers must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("MaxConcurrentUsers cannot exceed 10000");

        RuleFor(x => x.ReleaseRatePerMinute)
            .GreaterThan(0).WithMessage("ReleaseRatePerMinute must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("ReleaseRatePerMinute cannot exceed 1000");
    }
}
