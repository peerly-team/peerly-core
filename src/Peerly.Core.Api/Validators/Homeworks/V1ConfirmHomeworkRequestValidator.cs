using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1ConfirmHomeworkRequestValidator : AbstractValidator<V1ConfirmHomeworkRequest>
{
    public V1ConfirmHomeworkRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleForEach(x => x.MarkCorrections)
            .ChildRules(correction =>
            {
                correction.RuleFor(x => x.SubmittedHomeworkId)
                    .GreaterThan(0);

                correction.RuleFor(x => x.TeacherMark)
                    .InclusiveBetween(0, 100);
            });
    }
}
