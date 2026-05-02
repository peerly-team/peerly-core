using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

internal sealed class GetStudentCourseHandler : IQueryHandler<GetStudentCourseQuery, GetStudentCourseQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<GetStudentCourseQuery, GetStudentCourseQueryResponse> _validator;

    public GetStudentCourseHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IQueryValidator<GetStudentCourseQuery, GetStudentCourseQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<GetStudentCourseQueryResponse> ExecuteAsync(GetStudentCourseQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var course = await unitOfWork.ReadOnlyCourseRepository.GetAsync(query.CourseId, cancellationToken);
        var homeworkCount = await unitOfWork.ReadOnlyHomeworkRepository.GetHomeworkCountAsync(query.CourseId, cancellationToken);
        var studentCount = await GetStudentCountAsync(query.CourseId, unitOfWork, cancellationToken);

        return new GetStudentCourseQueryResponse
        {
            Course = course!,
            StudentCount = studentCount,
            HomeworkCount = homeworkCount
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
