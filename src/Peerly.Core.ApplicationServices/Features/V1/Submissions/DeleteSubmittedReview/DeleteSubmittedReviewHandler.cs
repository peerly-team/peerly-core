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
    private readonly ICommandValidator<DeleteSubmittedReviewCommand, Success> _validator;

    public DeleteSubmittedReviewHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<DeleteSubmittedReviewCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteSubmittedReviewCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.SubmittedReviewScoreRepository.DeleteBySubmittedReviewIdAsync(command.SubmittedReviewId, cancellationToken);
        await unitOfWork.SubmittedReviewRepository.DeleteAsync(command.SubmittedReviewId, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
