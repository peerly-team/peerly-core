using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1GetSubmittedHomeworkRequestValidator : AbstractValidator<V1GetSubmittedHomeworkRequest>
{
    public V1GetSubmittedHomeworkRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
