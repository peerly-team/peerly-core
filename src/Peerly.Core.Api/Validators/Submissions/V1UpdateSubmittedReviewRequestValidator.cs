using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1UpdateSubmittedReviewRequestValidator : AbstractValidator<V1UpdateSubmittedReviewRequest>
{
    public V1UpdateSubmittedReviewRequestValidator()
    {
        RuleFor(x => x.SubmittedReviewId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.Mark)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.Comment)
            .NotEmpty();
    }
}
