using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1ListTeacherCourseHomeworksRequestValidator : AbstractValidator<V1ListTeacherCourseHomeworksRequest>
{
    public V1ListTeacherCourseHomeworksRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.CourseId)
            .GreaterThan(0);
    }
}
