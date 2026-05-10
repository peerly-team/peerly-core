using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

internal sealed class DeleteSubmittedHomeworkFileValidator : ICommandValidator<DeleteSubmittedHomeworkFileCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public DeleteSubmittedHomeworkFileValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(DeleteSubmittedHomeworkFileCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null || submittedHomework.StudentId != command.StudentId)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound);
        }

        var isFileExists = await unitOfWork.ReadOnlySubmittedHomeworkFileRepository.ExistsAsync(command.SubmittedHomeworkId, command.FileId, cancellationToken);
        if (!isFileExists)
        {
            return OtherError.NotFound(SubmittedHomeworkErrors.SubmittedHomeworkFileNotFound);
        }

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        if (homework.Status is not HomeworkStatus.Published)
        {
            return ValidationError.From(HomeworkErrors.HomeworkNotAcceptingSubmissions);
        }

        return CommandValidationResult.Ok();
    }
}
