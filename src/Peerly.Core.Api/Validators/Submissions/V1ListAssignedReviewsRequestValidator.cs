using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1ListAssignedReviewsRequestValidator : AbstractValidator<V1ListAssignedReviewsRequest>
{
    public V1ListAssignedReviewsRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
