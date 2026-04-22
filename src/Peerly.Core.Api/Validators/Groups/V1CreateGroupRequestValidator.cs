using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Groups;

internal sealed class V1CreateGroupRequestValidator : AbstractValidator<V1CreateGroupRequest>
{
    public V1CreateGroupRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
