using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;

internal sealed class AddGroupStudentHandler : ICommandHandler<AddGroupStudentCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IAddGroupStudentValidator _validator;
    private readonly IClock _clock;

    public AddGroupStudentHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IAddGroupStudentValidator validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        AddGroupStudentCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var groupStudentAddItem = command.ToGroupStudentAddItem(_clock.GetCurrentTime());
        await unitOfWork.GroupStudentRepository.AddAsync(groupStudentAddItem, cancellationToken);

        return new Success();
    }
}
