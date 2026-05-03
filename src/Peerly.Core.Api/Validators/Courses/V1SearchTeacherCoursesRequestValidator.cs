using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Courses;

internal sealed class V1SearchTeacherCoursesRequestValidator : AbstractValidator<V1SearchTeacherCoursesRequest>
{
    public V1SearchTeacherCoursesRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.PaginationInfo.Offset)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaginationInfo.PageSize)
            .GreaterThanOrEqualTo(0);
    }
}
