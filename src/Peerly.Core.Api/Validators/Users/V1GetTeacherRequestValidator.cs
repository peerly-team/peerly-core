using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Users;

internal sealed class V1GetTeacherRequestValidator : AbstractValidator<V1GetTeacherRequest>
{
    public V1GetTeacherRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
