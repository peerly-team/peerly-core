using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupStudent;

internal sealed class AddGroupStudentHandler : ICommandHandler<AddGroupStudentCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public AddGroupStudentHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        AddGroupStudentCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        // todo: добавить проверку, что группа существует
        // todo: добавить проверку, что студент существует
        // todo: добавить проверку, что препод может добавлять студентов в группу

        var existingStudents = await unitOfWork.GroupStudentRepository.ListAsync(
            new GroupStudentFilter
            {
                GroupIds = [command.GroupId],
                StudentIds = [command.StudentId]
            },
            cancellationToken);

        if (existingStudents.Count != 0)
        {
            return OtherError.Conflict();
        }

        var groupStudentAddItem = new GroupStudentAddItem
        {
            GroupId = command.GroupId,
            StudentId = command.StudentId,
            CreationTime = _clock.GetCurrentTime()
        };
        await unitOfWork.GroupStudentRepository.AddAsync(groupStudentAddItem, cancellationToken);

        return new Success();
    }
}
