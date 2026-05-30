using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;

internal sealed class AddGroupTeacherHandler : ICommandHandler<AddGroupTeacherCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<AddGroupTeacherCommand, Success> _validator;
    private readonly IClock _clock;

    public AddGroupTeacherHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<AddGroupTeacherCommand, Success> validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        AddGroupTeacherCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        var groupTeacherAddItem = command.ToGroupTeacherAddItem(_clock.GetCurrentTime());
        await unitOfWork.GroupTeacherRepository.AddAsync(groupTeacherAddItem, cancellationToken);

        return new Success();
    }
}
