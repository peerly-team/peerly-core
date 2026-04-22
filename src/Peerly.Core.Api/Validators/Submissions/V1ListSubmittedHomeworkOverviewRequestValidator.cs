using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1ListSubmittedHomeworkOverviewRequestValidator : AbstractValidator<V1ListSubmittedHomeworkOverviewRequest>
{
    public V1ListSubmittedHomeworkOverviewRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
