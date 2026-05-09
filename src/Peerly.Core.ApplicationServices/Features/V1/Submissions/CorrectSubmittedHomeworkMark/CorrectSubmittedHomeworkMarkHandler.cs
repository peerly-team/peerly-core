using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

internal sealed class CorrectSubmittedHomeworkMarkHandler : ICommandHandler<CorrectSubmittedHomeworkMarkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<CorrectSubmittedHomeworkMarkCommand, Success> _validator;
    private readonly IClock _clock;

    public CorrectSubmittedHomeworkMarkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IClock clock,
        ICommandValidator<CorrectSubmittedHomeworkMarkCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        CorrectSubmittedHomeworkMarkCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var isSuccess = await unitOfWork.SubmittedHomeworkMarkRepository.UpdateAsync(
            command.SubmittedHomeworkId,
            builder => builder
                .Set(item => item.TeacherMark, command.TeacherMark)
                .Set(item => item.TeacherId, (long)command.TeacherId),
            cancellationToken);

        return isSuccess
            ? new Success()
            : OtherError.NotFound();
    }
}
