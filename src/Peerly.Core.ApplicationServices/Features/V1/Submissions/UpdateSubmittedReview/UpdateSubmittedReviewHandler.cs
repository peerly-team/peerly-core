using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;

internal sealed class UpdateSubmittedReviewHandler : ICommandHandler<UpdateSubmittedReviewCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<UpdateSubmittedReviewCommand, Success> _validator;

    public UpdateSubmittedReviewHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<UpdateSubmittedReviewCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(UpdateSubmittedReviewCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var mark = command.Scores.Sum(s => s.Score);
        await unitOfWork.SubmittedReviewRepository.UpdateAsync(
            command.SubmittedReviewId,
            builder => builder
                .Set(item => item.Mark, mark)
                .Set(item => item.Comment, command.Comment),
            cancellationToken);

        await unitOfWork.SubmittedReviewScoreRepository.DeleteBySubmittedReviewIdAsync(command.SubmittedReviewId, cancellationToken);

        var scoreAddItems = command.Scores.ToSubmittedReviewScoreAddItems(command.SubmittedReviewId);
        await unitOfWork.SubmittedReviewScoreRepository.BatchAddAsync(scoreAddItems, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
