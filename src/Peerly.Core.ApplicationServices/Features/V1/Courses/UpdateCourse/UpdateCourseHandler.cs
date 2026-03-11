using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;

internal sealed class UpdateCourseHandler : ICommandHandler<UpdateCourseCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public UpdateCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        if (command.Status is CourseStatus.Deleted)
        {
            return ValidationError.From(CourseErrors.ForbiddenUpdateCourseStatusToDelete);
        }

        var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var course = await unitOfWork.CourseRepository.GetAsync(command.CourseId, cancellationToken);
        if (course is null)
        {
            return OtherError.NotFound(CourseErrors.CourseNotFound);
        }

        if (course.Status is not (CourseStatus.Draft or CourseStatus.InProgress))
        {
            return ValidationError.From(CourseErrors.IncorrectCourseStatusForUpdate);
        }

        var courseTeacherExistsItem = command.ToCourseTeacherExistsItem();
        var isCourseTeacherExists = await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacherExists)
        {
            return OtherError.PermissionDenied();
        }

        _ = await unitOfWork.CourseRepository.UpdateAsync(
            command.CourseId,
            builder => builder
                .Set(item => item.Name, command.Name)
                .Set(item => item.Description, command.Description)
                .Set(item => item.Status, command.Status),
            cancellationToken);

        return new Success();
    }
}
