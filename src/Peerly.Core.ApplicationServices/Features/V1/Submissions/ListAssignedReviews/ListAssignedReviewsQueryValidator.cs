using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;

internal sealed class ListAssignedReviewsQueryValidator : IQueryValidator<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public ListAssignedReviewsQueryValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task ValidateAsync(ListAssignedReviewsQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);
        if (homework?.Status is not HomeworkStatus.Reviewing)
        {
            throw new NotFoundException();
        }

        if (homework.GroupId is { } groupId)
        {
            var groupStudent = query.ToGroupStudent(groupId);
            var isGroupStudentExists = await unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, cancellationToken);
            if (!isGroupStudentExists)
            {
                throw new NotFoundException();
            }
        }
        else
        {
            var courseStudent = query.ToCourseStudent(homework.CourseId);
            var isCourseStudentExists = await unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, cancellationToken);
            if (!isCourseStudentExists)
            {
                throw new NotFoundException();
            }
        }
    }
}
