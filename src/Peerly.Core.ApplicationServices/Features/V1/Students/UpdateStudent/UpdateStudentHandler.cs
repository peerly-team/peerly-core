using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;

internal sealed class UpdateStudentHandler : ICommandHandler<UpdateStudentCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<UpdateStudentCommand, Success> _validator;

    public UpdateStudentHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICommandValidator<UpdateStudentCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        UpdateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        _ = await unitOfWork.StudentRepository.UpdateAsync(
            command.StudentId,
            builder => builder.Set(item => item.Name, command.Name),
            cancellationToken);

        return new Success();
    }
}
