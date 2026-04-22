using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1GetSubmittedReviewRequestValidator : AbstractValidator<V1GetSubmittedReviewRequest>
{
    public V1GetSubmittedReviewRequestValidator()
    {
        RuleFor(x => x.SubmittedReviewId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
