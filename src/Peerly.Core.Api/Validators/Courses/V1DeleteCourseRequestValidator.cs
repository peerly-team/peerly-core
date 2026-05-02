using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Courses;

internal sealed class V1DeleteCourseRequestValidator : AbstractValidator<V1DeleteCourseRequest>
{
    public V1DeleteCourseRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
