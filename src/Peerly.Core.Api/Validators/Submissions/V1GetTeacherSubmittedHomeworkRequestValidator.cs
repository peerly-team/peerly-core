using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1GetTeacherSubmittedHomeworkRequestValidator : AbstractValidator<V1GetTeacherSubmittedHomeworkRequest>
{
    public V1GetTeacherSubmittedHomeworkRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
