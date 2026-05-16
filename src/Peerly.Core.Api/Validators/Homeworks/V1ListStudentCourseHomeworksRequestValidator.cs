using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1ListStudentCourseHomeworksRequestValidator : AbstractValidator<V1ListStudentCourseHomeworksRequest>
{
    public V1ListStudentCourseHomeworksRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.CourseId)
            .GreaterThan(0);
    }
}
