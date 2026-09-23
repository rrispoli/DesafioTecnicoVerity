using FluentValidation;

namespace Entries.Application.Features.Entries.Get;

public class GetEntryQueryValidator : AbstractValidator<GetEntryQuery>
{
    public GetEntryQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
