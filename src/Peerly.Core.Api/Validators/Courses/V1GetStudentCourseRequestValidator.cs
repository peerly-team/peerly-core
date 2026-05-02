using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Courses;

internal sealed class V1GetStudentCourseRequestValidator : AbstractValidator<V1GetStudentCourseRequest>
{
    public V1GetStudentCourseRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
