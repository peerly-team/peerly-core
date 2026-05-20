using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;

internal sealed class ListStudentCourseHomeworksHandler : IQueryHandler<ListStudentCourseHomeworksQuery, ListStudentCourseHomeworksQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListStudentCourseHomeworksHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListStudentCourseHomeworksQueryResponse> ExecuteAsync(
        ListStudentCourseHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseStudent = query.ToCourseStudent();
        var homeworks = await unitOfWork.ReadOnlyHomeworkRepository.ListStudentHomeworkInfosAsync(courseStudent, cancellationToken);

        return new ListStudentCourseHomeworksQueryResponse
        {
            StudentHomeworks = homeworks
        };
    }
}
