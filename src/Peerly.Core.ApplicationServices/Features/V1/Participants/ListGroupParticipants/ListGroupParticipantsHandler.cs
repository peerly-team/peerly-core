using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Teachers;
using Peerly.Core.Tools;
using Mapper = Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants.ListGroupParticipantsHandlerMapper;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants;

internal sealed class ListGroupParticipantsHandler : IQueryHandler<ListGroupParticipantsQuery, ListGroupParticipantsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListGroupParticipantsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListGroupParticipantsQueryResponse> ExecuteAsync(
        ListGroupParticipantsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        if (!await unitOfWork.ReadOnlyGroupRepository.ExistsAsync(query.GroupId, cancellationToken))
        {
            throw new NotFoundException();
        }

        var teachers = await GetTeachersAsync(query, unitOfWork, cancellationToken);
        var students = await GetStudentsAsync(query, unitOfWork, cancellationToken);

        return new ListGroupParticipantsQueryResponse
        {
            Teachers = teachers,
            Students = students
        };
    }

    private static async Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(
        ListGroupParticipantsQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var teacherIds = await unitOfWork.ReadOnlyGroupTeacherRepository.ListTeacherIdAsync(query.GroupId, cancellationToken);
        if (teacherIds.Count == 0)
        {
            return [];
        }

        var filter = Mapper.ToTeacherFilter(teacherIds);
        return await unitOfWork.ReadOnlyTeacherRepository.ListAsync(filter, cancellationToken);
    }

    private static async Task<IReadOnlyCollection<Student>> GetStudentsAsync(
        ListGroupParticipantsQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(
            Mapper.ToGroupStudentFilter(query.GroupId),
            cancellationToken);
        if (groupStudents.Count == 0)
        {
            return [];
        }

        var studentIds = groupStudents.ToArrayBy(groupStudent => groupStudent.StudentId);
        var studentFilter = Mapper.ToStudentFilter(studentIds);
        return await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);
    }
}
