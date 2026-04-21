using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1DeleteSubmittedHomeworkRequestValidator : AbstractValidator<V1DeleteSubmittedHomeworkRequest>
{
    public V1DeleteSubmittedHomeworkRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
