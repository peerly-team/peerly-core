using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

internal sealed class GetTeacherCourseHandler : IQueryHandler<GetTeacherCourseQuery, GetTeacherCourseQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<GetTeacherCourseQuery, GetTeacherCourseQueryResponse> _validator;

    public GetTeacherCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IQueryValidator<GetTeacherCourseQuery, GetTeacherCourseQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<GetTeacherCourseQueryResponse> ExecuteAsync(GetTeacherCourseQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var course = await unitOfWork.ReadOnlyCourseRepository.GetAsync(query.CourseId, cancellationToken);
        var homeworkCount = await unitOfWork.ReadOnlyHomeworkRepository.GetHomeworkCountAsync(query.CourseId, cancellationToken);
        var studentCount = await GetStudentCountAsync(query.CourseId, unitOfWork, cancellationToken);

        return new GetTeacherCourseQueryResponse
        {
            CourseInfo = new CourseQueryResponseItem
            {
                Course = course!,
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
