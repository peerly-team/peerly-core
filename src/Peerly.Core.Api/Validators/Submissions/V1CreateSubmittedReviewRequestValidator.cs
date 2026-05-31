using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1CreateSubmittedReviewRequestValidator : AbstractValidator<V1CreateSubmittedReviewRequest>
{
    public V1CreateSubmittedReviewRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.Scores)
            .NotEmpty();

        RuleForEach(x => x.Scores)
            .ChildRules(score =>
            {
                score.RuleFor(s => s.RubricCriterionId)
                    .GreaterThan(0);

                score.RuleFor(s => s.Score)
                    .GreaterThanOrEqualTo(0);
            });

        RuleFor(x => x.Comment)
            .NotEmpty();
    }
}
