using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Teachers;

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
        var studentCount = await GetStudentCountAsync(unitOfWork, query.CourseId, cancellationToken);
        var files = await unitOfWork.ReadOnlyCourseFileRepository.ListFilesAsync(query.CourseId, cancellationToken);
        var teachers = await GetCourseTeachersAsync(unitOfWork, query.CourseId, cancellationToken);

        return new GetTeacherCourseQueryResponse
        {
            Course = course!,
            StudentCount = studentCount,
            HomeworkCount = homeworkCount,
            Files = files,
            Teachers = teachers
        };
    }

    private static async Task<int> GetStudentCountAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        CourseId courseId,
        CancellationToken cancellationToken)
    {
        var filter = GroupFilter.Empty() with { CourseIds = [courseId] };
        var groups = await unitOfWork.ReadOnlyGroupRepository.ListAsync(filter, cancellationToken);

        return groups.Sum(group => group.StudentCount);
    }

    private static async Task<Teacher[]> GetCourseTeachersAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        CourseId courseId,
        CancellationToken cancellationToken)
    {
        var teacherIds = await unitOfWork.ReadOnlyCourseTeacherRepository.ListTeacherIdAsync(courseId, cancellationToken);
        if (teacherIds.Count == 0)
            return [];

        var filter = new TeacherFilter { TeacherIds = teacherIds };
        var teachers = await unitOfWork.ReadOnlyTeacherRepository.ListAsync(filter, cancellationToken);

        return teachers.ToArray();
    }
}
