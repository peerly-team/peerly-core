using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;

internal sealed class
    ListTeacherCourseHomeworksHandler : IQueryHandler<ListTeacherCourseHomeworksQuery, ListTeacherCourseHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ITeacherCourseAccessChecker _teacherCourseAccessChecker;

    public ListTeacherCourseHomeworksHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ITeacherCourseAccessChecker teacherCourseAccessChecker)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _teacherCourseAccessChecker = teacherCourseAccessChecker;
    }

    public async Task<ListTeacherCourseHomeworksQueryResponse> ExecuteAsync(
        ListTeacherCourseHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseTeacher = query.ToCourseTeacher();
        if (!await _teacherCourseAccessChecker.RunAsync(courseTeacher, cancellationToken))
        {
            throw new NotFoundException();
        }

        var homeworkFilter = query.ToHomeworkFilter();
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListAsync(homeworkFilter, cancellationToken);

        return new ListTeacherCourseHomeworksQueryResponse
        {
            Homeworks = homeworks
        };
    }
}
