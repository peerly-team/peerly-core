using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Students;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.SearchGroupStudents;

internal sealed class SearchGroupStudentsHandler : IQueryHandler<SearchGroupStudentsQuery, SearchGroupStudentsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public SearchGroupStudentsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<SearchGroupStudentsQueryResponse> ExecuteAsync(
        SearchGroupStudentsQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var groupFilter = GroupFilter.Empty() with { GroupIds = [query.GroupId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);
        if (groups.Count == 0)
        {
            throw new NotFoundException();
        }

        var group = groups.First();

        var courseTeacherExistsItem = new CourseTeacherExistsItem
        {
            CourseId = group.CourseId,
            TeacherId = query.TeacherId
        };
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacher)
        {
            throw new NotFoundException();
        }

        var groupStudentFilter = GroupStudentFilter.Empty() with { GroupIds = [query.GroupId] };
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        if (groupStudents.Count == 0)
        {
            return new SearchGroupStudentsQueryResponse { Students = [] };
        }

        var studentFilter = new StudentFilter
        {
            StudentIds = groupStudents.ToArrayBy(gs => gs.StudentId)
        };
        var students = await unitOfWork.ReadOnlyStudentRepository.ListAsync(studentFilter, cancellationToken);

        return new SearchGroupStudentsQueryResponse
        {
            Students = students
        };
    }
}
