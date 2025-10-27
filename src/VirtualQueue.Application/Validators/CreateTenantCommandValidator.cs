using FluentValidation;

namespace VirtualQueue.Application.Validators;

public class CreateTenantCommandValidator : AbstractValidator<Commands.Tenants.CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domain is required")
            .MaximumLength(255).WithMessage("Domain cannot exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Domain must be a valid domain name");
    }
}
