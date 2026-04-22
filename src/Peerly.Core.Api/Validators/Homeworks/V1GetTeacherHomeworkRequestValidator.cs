using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1GetTeacherHomeworkRequestValidator : AbstractValidator<V1GetTeacherHomeworkRequest>
{
    public V1GetTeacherHomeworkRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
