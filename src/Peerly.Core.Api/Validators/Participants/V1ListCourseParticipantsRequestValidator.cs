using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Participants;

internal sealed class V1ListCourseParticipantsRequestValidator : AbstractValidator<V1ListCourseParticipantsRequest>
{
    public V1ListCourseParticipantsRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);
    }
}
