using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Rubrics;

internal sealed class V1ListTeacherRubricsRequestValidator : AbstractValidator<V1ListTeacherRubricsRequest>
{
    public V1ListTeacherRubricsRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
