using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;

internal sealed class UpdateSubmittedReviewHandler : ICommandHandler<UpdateSubmittedReviewCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public UpdateSubmittedReviewHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        UpdateSubmittedReviewCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: permission — StudentId должен быть автором рецензии SubmittedReviewId.
        // Потребуется IReadOnlySubmittedReviewRepository.GetAuthorAsync(SubmittedReviewId) -> StudentId?
        // или GetAsync(SubmittedReviewId) -> SubmittedReview?. PermissionDenied до проверки существования.
        // TODO: homework во «фазе ревью», ReviewDeadline не истёк — иначе ValidationError.
        // TODO: вынести проверки в IUpdateSubmittedReviewValidator + UpdateSubmittedReviewValidator
        // + UpdateSubmittedReviewInstaller по паттерну CreateSubmittedReviewValidator.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        await unitOfWork.SubmittedReviewRepository.UpdateAsync(
            command.SubmittedReviewId,
            builder => builder
                .Set(item => item.Mark, command.Mark)
                .Set(item => item.Comment, command.Comment),
            cancellationToken);

        return new Success();
    }
}
