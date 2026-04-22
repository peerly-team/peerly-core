using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1CorrectSubmittedHomeworkMarkRequestValidator : AbstractValidator<V1CorrectSubmittedHomeworkMarkRequest>
{
    public V1CorrectSubmittedHomeworkMarkRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherMark)
            .InclusiveBetween(0, 100);
    }
}
