using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Participants;

internal sealed class V1AddGroupStudentRequestValidator : AbstractValidator<V1AddGroupStudentRequest>
{
    public V1AddGroupStudentRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
