using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;

internal sealed class ListTeacherCourseHomeworksHandler : IQueryHandler<ListTeacherCourseHomeworksQuery, ListTeacherCourseHomeworksQueryResponse>
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

        var courseTeacherExistsItem = query.ToCourseTeacherExistsItem();
        var isTeacherExists = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isTeacherExists)
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
