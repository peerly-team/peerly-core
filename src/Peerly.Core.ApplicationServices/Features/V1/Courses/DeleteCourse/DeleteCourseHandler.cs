using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;

internal sealed class DeleteCourseHandler : ICommandHandler<DeleteCourseCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public DeleteCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var courseTeacherExistsItem = command.ToCourseTeacherExistsItem();
        var isCourseTeacherExists = await unitOfWork.CourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacherExists)
        {
            return OtherError.PermissionDenied();
        }

        var course = await unitOfWork.CourseRepository.GetAsync(command.CourseId, cancellationToken);
        if (course is null)
        {
            return OtherError.NotFound(CourseErrors.CourseNotFound);
        }

        if (course.Status is not CourseStatus.Draft)
        {
            return ValidationError.From(CourseErrors.IncorrectCourseStatusForDelete);
        }

        // todo: подумать насчет перевода логики на удаление курса и всей связанной информации с ним (может, сделать это фоновой джобой)
        _ = await unitOfWork.CourseRepository.UpdateAsync(
            command.CourseId,
            builder => builder.Set(item => item.Status, CourseStatus.Deleted),
            cancellationToken);

        return new Success();
    }
}
