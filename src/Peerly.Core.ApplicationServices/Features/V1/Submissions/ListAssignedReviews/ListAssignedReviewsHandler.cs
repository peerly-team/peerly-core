using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;

internal sealed class ListAssignedReviewsHandler : IQueryHandler<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public ListAssignedReviewsHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<ListAssignedReviewsQueryResponse> ExecuteAsync(ListAssignedReviewsQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        // TODO: реализовать permission-check по образцу GetStudentHomeworkHandler:
        // 1. IStudentCourseAccessChecker — студент имеет доступ к курсу homework'а.
        // 2. Если homework групповой — студент состоит в этой группе.
        // 3. homework.Status == HomeworkStatus.Reviewing (до стадии рецензирования
        //    назначений нет; уточнить с владельцем продукта, должен ли Closed тоже
        //    возвращать пустой список).
        // Все несоответствия — NotFoundException (не раскрываем существование).

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken)
                       ?? throw new NotFoundException();
        var assignedIds = await unitOfWork.ReadOnlyDistributionReviewerRepository.ListAssignedByAsync(query.StudentId, query.HomeworkId, cancellationToken);
        var reviewedIds = await unitOfWork.ReadOnlySubmittedReviewRepository.ListReviewedByAsync(query.StudentId, query.HomeworkId, cancellationToken);

        var reviewedSet = reviewedIds.ToHashSet();
        return new ListAssignedReviewsQueryResponse
        {
            AssignedReviews = assignedIds.ToArrayBy(
                id => new AssignedReview
                {
                    SubmittedHomeworkId = id,
                    HomeworkName = homework.Name,
                    IsReviewed = reviewedSet.Contains(id)
                })
        };
    }
}
