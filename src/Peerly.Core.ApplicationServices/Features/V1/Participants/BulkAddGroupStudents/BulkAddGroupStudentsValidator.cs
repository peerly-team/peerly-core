using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;

internal sealed class BulkAddGroupStudentsValidator : ICommandValidator<BulkAddGroupStudentsCommand, BulkAddGroupStudentsCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public BulkAddGroupStudentsValidator(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(BulkAddGroupStudentsCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var group = await unitOfWork.ReadOnlyGroupRepository.GetAsync(command.GroupId, cancellationToken);
        if (group is null)
        {
            return OtherError.NotFound();
        }

        var courseTeacher = command.ToCourseTeacher(group.CourseId);
        if (!await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken))
        {
            return OtherError.PermissionDenied();
        }

        var teacher = await unitOfWork.ReadOnlyTeacherRepository.GetAsync(command.TeacherId, cancellationToken);
        if (teacher is null)
        {
            return OtherError.NotFound();
        }

        return CommandValidationResult.Ok();
    }
}
