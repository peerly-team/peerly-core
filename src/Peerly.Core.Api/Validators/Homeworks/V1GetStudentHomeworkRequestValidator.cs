using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1GetStudentHomeworkRequestValidator : AbstractValidator<V1GetStudentHomeworkRequest>
{
    public V1GetStudentHomeworkRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
