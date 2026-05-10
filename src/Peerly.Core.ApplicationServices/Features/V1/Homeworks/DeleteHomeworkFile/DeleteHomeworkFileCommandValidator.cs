using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomeworkFile;

internal sealed class DeleteHomeworkFileCommandValidator : ICommandValidator<DeleteHomeworkFileCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public DeleteHomeworkFileCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(DeleteHomeworkFileCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var homework = await unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        if (homework.TeacherId != command.TeacherId)
        {
            return OtherError.PermissionDenied();
        }

        if (homework.Status is not HomeworkStatus.Draft)
        {
            return ValidationError.From(HomeworkErrors.IncorrectHomeworkStatusForDeleteFile);
        }

        return CommandValidationResult.Ok();
    }
}
