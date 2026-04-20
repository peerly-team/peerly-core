using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

internal sealed class GetStudentCourseHandler : IQueryHandler<GetStudentCourseQuery, GetStudentCourseQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IStudentCourseAccessChecker _studentCourseAccessChecker;

    public GetStudentCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IStudentCourseAccessChecker studentCourseAccessChecker)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _studentCourseAccessChecker = studentCourseAccessChecker;
    }

    public async Task<GetStudentCourseQueryResponse> ExecuteAsync(GetStudentCourseQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseStudent = query.ToCourseStudent();
        if (!await _studentCourseAccessChecker.RunAsync(courseStudent, cancellationToken))
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
