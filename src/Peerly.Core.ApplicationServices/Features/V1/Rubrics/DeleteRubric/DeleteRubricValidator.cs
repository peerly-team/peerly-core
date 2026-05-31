using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;

internal sealed class DeleteRubricValidator : ICommandValidator<DeleteRubricCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public DeleteRubricValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(
        DeleteRubricCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var rubric = await unitOfWork.ReadOnlyRubricRepository.GetAsync(command.RubricId, cancellationToken);
        if (rubric is null)
        {
            return OtherError.NotFound(RubricErrors.RubricNotFound);
        }

        if (rubric.TeacherId != command.TeacherId)
        {
            return OtherError.PermissionDenied();
        }

        if (await unitOfWork.ReadOnlyHomeworkRepository.ExistsByRubricIdAsync(command.RubricId, cancellationToken))
        {
            return ValidationError.From(RubricErrors.RubricReferencedByHomework);
        }

        return CommandValidationResult.Ok();
    }
}
