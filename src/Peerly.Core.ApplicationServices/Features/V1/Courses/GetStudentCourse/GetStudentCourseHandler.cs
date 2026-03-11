using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

internal sealed class GetStudentCourseHandler : IQueryHandler<GetStudentCourseQuery, GetStudentCourseQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetStudentCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetStudentCourseQueryResponse> ExecuteAsync(GetStudentCourseQuery query, CancellationToken cancellationToken)
    {
        var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var isCourseStudentExists = await IsCourseStudentExistsAsync(query, unitOfWork, cancellationToken);
        if (!isCourseStudentExists)
        {
            throw new NotFoundException();
        }

        var course = await unitOfWork.ReadOnlyCourseRepository.GetAsync(query.CourseId, cancellationToken);
        if (course is null)
        {
            throw new NotFoundException();
        }

        var homeworkCount = await unitOfWork.ReadOnlyHomeworkRepository.GetHomeworkCountAsync(query.CourseId, cancellationToken);
        var studentCount = await GetStudentCountAsync(query.CourseId, unitOfWork, cancellationToken);

        return new GetStudentCourseQueryResponse
        {
            CourseInfo = new CourseQueryResponseItem
            {
                Course = course,
                StudentCount = studentCount,
                HomeworkCount = homeworkCount
            }
        };
    }

    private static async Task<bool> IsCourseStudentExistsAsync(
        GetStudentCourseQuery query,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var groupFilter = GroupFilter.Empty() with { CourseIds = [query.CourseId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(groupFilter, cancellationToken);
        if (groups.Count == 0)
        {
            return false;
        }

        var groupStudentFilter = new GroupStudentFilter
        {
            GroupIds = groups.ToArrayBy(group => group.Id),
            StudentIds = [query.StudentId]
        };
        var groupStudents = await unitOfWork.ReadOnlyGroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);
        return groupStudents.Count > 0;
    }

    private static async Task<int> GetStudentCountAsync(
        CourseId courseId,
        ICommonReadOnlyUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var filter = GroupFilter.Empty() with { CourseIds = [courseId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(filter, cancellationToken);

        return groups.Sum(group => group.StudentCount);
    }
}
