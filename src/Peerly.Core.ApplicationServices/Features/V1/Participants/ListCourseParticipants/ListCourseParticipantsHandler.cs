using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Teachers;
using Peerly.Core.Tools;
using Mapper = Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants.ListCourseParticipantsHandlerMapper;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;

internal sealed class ListCourseParticipantsHandler : IQueryHandler<ListCourseParticipantsQuery, ListCourseParticipantsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListCourseParticipantsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListCourseParticipantsQueryResponse> ExecuteAsync(
        ListCourseParticipantsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        _ = await unitOfWork.ReadOnlyCourseRepository.GetAsync(query.CourseId, cancellationToken)
                     ?? throw new NotFoundException();

        var teachers = await GetTeachersAsync(query, unitOfWork, cancellationToken);
        var students = await GetStudentsAsync(query, unitOfWork, cancellationToken);

        return new ListCourseParticipantsQueryResponse
        {
            Teachers = teachers,
            Students = students
        };
    }

    private static async Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(
        ListCourseParticipantsQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var teacherIds = await unitOfWork.ReadOnlyCourseTeacherRepository.ListTeacherIdAsync(query.CourseId, cancellationToken);
        if (teacherIds.Count == 0)
        {
            return [];
        }

        var filter = Mapper.ToTeacherFilter(teacherIds);
        return await unitOfWork.ReadOnlyTeacherRepository.ListAsync(filter, cancellationToken);
    }

    private static async Task<IReadOnlyCollection<Student>> GetStudentsAsync(
        ListCourseParticipantsQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(query.ToGroupFilter(), cancellationToken);
        if (groups.Count == 0)
        {
            return [];
        }

        var groupIds = groups.ToArrayBy(group => group.Id);
        var groupStudentFilter = Mapper.ToGroupStudentFilter(groupIds);
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);

        var studentIds = groupStudents
            .Select(groupStudent => groupStudent.StudentId)
            .Distinct()
            .ToArray();

        if (studentIds.Length == 0)
        {
            return [];
        }

        var studentFilter = Mapper.ToStudentFilter(studentIds);
        return await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);
    }
}
