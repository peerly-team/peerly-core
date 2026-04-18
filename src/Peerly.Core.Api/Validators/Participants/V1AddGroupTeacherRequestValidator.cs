using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Participants;

internal sealed class V1AddGroupTeacherRequestValidator : AbstractValidator<V1AddGroupTeacherRequest>
{
    public V1AddGroupTeacherRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.ActorTeacherId)
            .GreaterThan(0);
    }
}
