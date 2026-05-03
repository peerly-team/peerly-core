using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Courses;

internal sealed class V1UpdateCourseRequestValidator : AbstractValidator<V1UpdateCourseRequest>
{
    public V1UpdateCourseRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty();
    }
}
