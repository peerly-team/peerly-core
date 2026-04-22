using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;

internal sealed class DeleteSubmittedReviewHandler : ICommandHandler<DeleteSubmittedReviewCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public DeleteSubmittedReviewHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteSubmittedReviewCommand command, CancellationToken cancellationToken)
    {
        // TODO: permission — StudentId должен быть автором рецензии SubmittedReviewId.
        // Потребуется IReadOnlySubmittedReviewRepository.GetAsync(SubmittedReviewId) -> SubmittedReview?
        // (или тонкий GetAuthorAsync(SubmittedReviewId) -> StudentId?). PermissionDenied раньше NotFound
        // (правило permission-before-existence, образец — DeleteCourseHandler).
        // TODO: homework во «фазе ревью», ReviewDeadline не истёк — иначе ValidationError.
        // TODO: вынести проверки в IDeleteSubmittedReviewValidator + DeleteSubmittedReviewValidator
        // + DeleteSubmittedReviewInstaller по образцу DeleteSubmittedHomeworkValidator.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        await unitOfWork.SubmittedReviewRepository.DeleteAsync(command.SubmittedReviewId, cancellationToken);

        return new Success();
    }
}
