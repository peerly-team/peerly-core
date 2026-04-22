using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1DeleteSubmittedReviewRequestValidator : AbstractValidator<V1DeleteSubmittedReviewRequest>
{
    public V1DeleteSubmittedReviewRequestValidator()
    {
        RuleFor(x => x.SubmittedReviewId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
