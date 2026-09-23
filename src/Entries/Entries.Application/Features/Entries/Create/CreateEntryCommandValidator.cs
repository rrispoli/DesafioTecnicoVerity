using FluentValidation;

namespace Entries.Application.Features.Entries.Create;

public class CreateEntryCommandValidator : AbstractValidator<CreateEntryCommand>
{
    public CreateEntryCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid entry type.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.OccurredAt)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow).WithMessage("OccurredAt cannot be in the future.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
