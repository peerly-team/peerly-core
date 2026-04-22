using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Groups;

internal sealed class V1UpdateGroupRequestValidator : AbstractValidator<V1UpdateGroupRequest>
{
    public V1UpdateGroupRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
