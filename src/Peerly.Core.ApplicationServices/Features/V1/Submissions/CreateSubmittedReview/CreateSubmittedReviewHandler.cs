using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

internal sealed class CreateSubmittedReviewHandler : ICommandHandler<CreateSubmittedReviewCommand, CreateSubmittedReviewCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;
    private readonly ICreateSubmittedReviewValidator _validator;

    public CreateSubmittedReviewHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IClock clock,
        ICreateSubmittedReviewValidator validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
        _validator = validator;
    }

    public async Task<CommandResponse<CreateSubmittedReviewCommandResponse>> ExecuteAsync(
        CreateSubmittedReviewCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var submittedReviewAddItem = new SubmittedReviewAddItem
        {
            SubmittedHomeworkId = command.SubmittedHomeworkId,
            StudentId = command.StudentId,
            Mark = command.Mark,
            Comment = command.Comment,
            CreationTime = _clock.GetCurrentTime()
        };
        var submittedReviewId = await unitOfWork.SubmittedReviewRepository.AddAsync(submittedReviewAddItem, cancellationToken);

        return new CreateSubmittedReviewCommandResponse
        {
            SubmittedReviewId = submittedReviewId
        };
    }
}
