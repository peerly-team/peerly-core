using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Rubrics;

internal sealed class V1GetStudentRubricRequestValidator : AbstractValidator<V1GetStudentRubricRequest>
{
    public V1GetStudentRubricRequestValidator()
    {
        RuleFor(x => x.RubricId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
