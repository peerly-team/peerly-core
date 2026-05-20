using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;

internal sealed class
    ListTeacherCourseHomeworksHandler : IQueryHandler<ListTeacherCourseHomeworksQuery, ListTeacherCourseHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListTeacherCourseHomeworksHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListTeacherCourseHomeworksQueryResponse> ExecuteAsync(
        ListTeacherCourseHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseTeacher = query.ToCourseTeacher();
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListTeacherHomeworkInfosAsync(courseTeacher, cancellationToken);

        return new ListTeacherCourseHomeworksQueryResponse
        {
            Homeworks = homeworks
        };
    }
}
