using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherCourseHomeworks;

internal sealed class SearchTeacherCourseHomeworksHandler : IQueryHandler<SearchTeacherCourseHomeworksQuery, SearchTeacherCourseHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public SearchTeacherCourseHomeworksHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<SearchTeacherCourseHomeworksQueryResponse> ExecuteAsync(
        SearchTeacherCourseHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseTeacherExistsItem = new CourseTeacherExistsItem
        {
            CourseId = query.CourseId,
            TeacherId = query.TeacherId
        };
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacher)
        {
            throw new NotFoundException();
        }

        var homeworkFilter = new HomeworkFilter
        {
            CourseIds = [query.CourseId],
            GroupIds = [],
            HomeworkStatuses = query.Filter.HomeworkStatuses
        };
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListAsync(homeworkFilter, cancellationToken);

        return new SearchTeacherCourseHomeworksQueryResponse
        {
            Homeworks = homeworks
        };
    }
}
