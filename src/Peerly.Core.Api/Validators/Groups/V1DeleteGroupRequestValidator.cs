using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Groups;

internal sealed class V1DeleteGroupRequestValidator : AbstractValidator<V1DeleteGroupRequest>
{
    public V1DeleteGroupRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
