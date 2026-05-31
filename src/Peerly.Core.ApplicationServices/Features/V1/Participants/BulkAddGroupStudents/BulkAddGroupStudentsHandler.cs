using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Participants;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;

internal sealed class BulkAddGroupStudentsHandler : ICommandHandler<BulkAddGroupStudentsCommand, BulkAddGroupStudentsCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<BulkAddGroupStudentsCommand, BulkAddGroupStudentsCommandResponse> _validator;
    private readonly IClock _clock;

    public BulkAddGroupStudentsHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<BulkAddGroupStudentsCommand, BulkAddGroupStudentsCommandResponse> validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<BulkAddGroupStudentsCommandResponse>> ExecuteAsync(
        BulkAddGroupStudentsCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var students = await unitOfWork.StudentRepository.ListAsync(command.ToStudentFilter(), cancellationToken);
        var existingGroupStudents = await unitOfWork.GroupStudentRepository.ListAsync(command.ToGroupStudentFilter(), cancellationToken);

        var existingStudentIds = students.Select(student => student.Id).ToHashSet();
        var alreadyInGroupStudentIds = existingGroupStudents.Select(groupStudent => groupStudent.StudentId).ToHashSet();
        var studentIdsToAdd = command.StudentIds
            .Where(studentId => existingStudentIds.Contains(studentId) && !alreadyInGroupStudentIds.Contains(studentId))
            .ToArray();

        var addedIds = studentIdsToAdd.Length == 0
            ? []
            : await unitOfWork.GroupStudentRepository.BulkAddAsync(
                command.ToGroupStudentBulkAddItem(studentIdsToAdd, _clock.GetCurrentTime()),
                cancellationToken);

        var addedIdSet = addedIds.ToHashSet();
        var skippedItems = command.StudentIds
            .Where(studentId => !addedIdSet.Contains(studentId))
            .ToArrayBy(studentId => new SkippedStudentInfo
            {
                Id = studentId,
                Reason = existingStudentIds.Contains(studentId)
                    ? SkippedStudentReason.AlreadyInGroup
                    : SkippedStudentReason.NotFound
            });

        return new BulkAddGroupStudentsCommandResponse
        {
            AddedStudentIds = addedIds,
            SkippedStudents = skippedItems
        };
    }
}
