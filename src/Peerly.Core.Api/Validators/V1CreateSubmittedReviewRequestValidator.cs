using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators;

internal sealed class V1CreateSubmittedReviewRequestValidator : AbstractValidator<V1CreateSubmittedReviewRequest>
{
    public V1CreateSubmittedReviewRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.Mark)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.Comment)
            .NotEmpty();
    }
}
