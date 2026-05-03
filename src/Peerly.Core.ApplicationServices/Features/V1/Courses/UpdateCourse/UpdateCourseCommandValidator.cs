using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;

internal sealed class UpdateCourseCommandValidator : ICommandValidator<UpdateCourseCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public UpdateCourseCommandValidator(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var courseTeacher = command.ToCourseTeacher();
        if (!await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return OtherError.PermissionDenied();
        }

        var course = await unitOfWork.CourseRepository.GetAsync(command.CourseId, cancellationToken);
        if (course is null)
        {
            return OtherError.NotFound(CourseErrors.CourseNotFound);
        }

        if (course.Status is not (CourseStatus.Draft or CourseStatus.InProgress))
        {
            return ValidationError.From(CourseErrors.IncorrectCourseStatusForUpdate);
        }

        return CommandValidationResult.Ok();
    }
}
