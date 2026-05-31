using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1UpdateSubmittedHomeworkRequestValidator : AbstractValidator<V1UpdateSubmittedHomeworkRequest>
{
    public V1UpdateSubmittedHomeworkRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
