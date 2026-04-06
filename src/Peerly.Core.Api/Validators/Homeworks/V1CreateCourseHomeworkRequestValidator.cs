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

        RuleFor(x => x.DiscrepancyThreshold)
            .InclusiveBetween(0, 100);
    }
}
