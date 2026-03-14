using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Exceptions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

internal sealed class GetTeacherCourseHandler : IQueryHandler<GetTeacherCourseQuery, GetTeacherCourseQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherCourseQueryResponse> ExecuteAsync(GetTeacherCourseQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseTeacherExistsItem = query.ToCourseTeacherExistsItem();
        var isCourseTeacherExists = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacherExists)
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

        return new GetTeacherCourseQueryResponse
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
