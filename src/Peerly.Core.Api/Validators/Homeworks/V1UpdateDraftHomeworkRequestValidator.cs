using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1UpdateDraftHomeworkRequestValidator : AbstractValidator<V1UpdateDraftHomeworkRequest>
{
    public V1UpdateDraftHomeworkRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.AmountOfReviewers)
            .GreaterThan(0);

        RuleFor(x => x.Checklist)
            .NotEmpty();

        RuleFor(x => x.Deadline)
            .NotNull();

        RuleFor(x => x.ReviewDeadline)
            .NotNull();

        RuleFor(x => x.DiscrepancyThreshold)
            .InclusiveBetween(0, 100);
    }
}
