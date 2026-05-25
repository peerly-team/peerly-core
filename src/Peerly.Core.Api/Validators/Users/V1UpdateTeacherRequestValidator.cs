using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Users;

internal sealed class V1UpdateTeacherRequestValidator : AbstractValidator<V1UpdateTeacherRequest>
{
    public V1UpdateTeacherRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty();
    }
}
