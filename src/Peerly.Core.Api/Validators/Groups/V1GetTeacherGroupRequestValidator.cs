using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Groups;

internal sealed class V1GetTeacherGroupRequestValidator : AbstractValidator<V1GetTeacherGroupRequest>
{
    public V1GetTeacherGroupRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
