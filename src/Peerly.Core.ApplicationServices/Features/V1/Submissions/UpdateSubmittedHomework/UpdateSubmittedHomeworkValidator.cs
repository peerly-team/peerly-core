using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.ApplicationServices.Validators.HomeworkSubmissionAccess.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;

internal sealed class UpdateSubmittedHomeworkValidator : IUpdateSubmittedHomeworkValidator
{
    private readonly IHomeworkSubmissionAccessValidator _accessValidator;
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public UpdateSubmittedHomeworkValidator(
        IHomeworkSubmissionAccessValidator accessValidator,
        ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _accessValidator = accessValidator;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<OtherError?> RunAsync(UpdateSubmittedHomeworkCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        if (submittedHomework.StudentId != command.StudentId)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        return await _accessValidator.RunAsync(submittedHomework.HomeworkId, command.StudentId, cancellationToken);
    }
}
