using FluentValidation;

namespace VirtualQueue.Application.Validators;

public class EnqueueUserCommandValidator : AbstractValidator<Commands.UserSessions.EnqueueUserCommand>
{
    public EnqueueUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required");

        RuleFor(x => x.QueueId)
            .NotEmpty().WithMessage("QueueId is required");

        RuleFor(x => x.UserIdentifier)
            .NotEmpty().WithMessage("UserIdentifier is required")
            .MaximumLength(255).WithMessage("UserIdentifier cannot exceed 255 characters");

        RuleFor(x => x.Metadata)
            .MaximumLength(1000).WithMessage("Metadata cannot exceed 1000 characters");
    }
}
