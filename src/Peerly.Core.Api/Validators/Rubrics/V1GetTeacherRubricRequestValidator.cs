using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Rubrics;

internal sealed class V1GetTeacherRubricRequestValidator : AbstractValidator<V1GetTeacherRubricRequest>
{
    public V1GetTeacherRubricRequestValidator()
    {
        RuleFor(x => x.RubricId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
