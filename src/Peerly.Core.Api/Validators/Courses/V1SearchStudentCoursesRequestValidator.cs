using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Courses;

internal sealed class V1SearchStudentCoursesRequestValidator : AbstractValidator<V1SearchStudentCoursesRequest>
{
    public V1SearchStudentCoursesRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.PaginationInfo.Offset)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaginationInfo.PageSize)
            .GreaterThanOrEqualTo(0);
    }
}
