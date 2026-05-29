using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Rubrics;

internal sealed class V1DeleteRubricRequestValidator : AbstractValidator<V1DeleteRubricRequest>
{
    public V1DeleteRubricRequestValidator()
    {
        RuleFor(x => x.RubricId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
