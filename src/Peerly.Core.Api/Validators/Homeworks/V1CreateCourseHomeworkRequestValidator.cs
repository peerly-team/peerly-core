using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1CreateCourseHomeworkRequestValidator : AbstractValidator<V1CreateCourseHomeworkRequest>
{
    public V1CreateCourseHomeworkRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.AmountOfReviewers)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.RubricId)
            .GreaterThan(0)
            .When(x => x.HasRubricId);

        RuleFor(x => x.DiscrepancyThreshold)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Deadline)
            .NotNull()
            .Must((request, deadline) => deadline < request.ReviewDeadline)
            .When(x => x.Deadline is not null && x.ReviewDeadline is not null, ApplyConditionTo.CurrentValidator)
            .WithMessage("'Deadline' must be less than 'Review Deadline'.");

        RuleFor(x => x.ReviewDeadline)
            .NotNull();
    }
}
