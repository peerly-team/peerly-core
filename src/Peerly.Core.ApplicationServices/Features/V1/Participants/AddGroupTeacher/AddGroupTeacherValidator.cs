using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;

internal sealed class AddGroupTeacherValidator : ICommandValidator<AddGroupTeacherCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public AddGroupTeacherValidator(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(AddGroupTeacherCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groupFilter = command.ToGroupFilter();
        var group = (await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken)).SingleOrDefault();
        if (group is null)
        {
            return OtherError.NotFound();
        }

        var teacherFilter = command.ToTeacherFilter();
        var teachers = await unitOfWork.ReadOnlyTeacherRepository.ListAsync(teacherFilter, cancellationToken);
        if (teachers.Count != teacherFilter.TeacherIds.Count)
        {
            return OtherError.NotFound();
        }

        var courseTeacherExistsItem = command.ToCourseTeacher(group.CourseId);
        var actorIsCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!actorIsCourseTeacher)
        {
            return OtherError.PermissionDenied();
        }

        var groupTeacherFilter = command.ToGroupTeacherFilter();
        var existing = await unitOfWork.ReadOnlyGroupTeacherRepository.ListAsync(groupTeacherFilter, cancellationToken);
        if (existing.Count != 0)
        {
            return OtherError.Conflict();
        }

        return CommandValidationResult.Ok();
    }
}
