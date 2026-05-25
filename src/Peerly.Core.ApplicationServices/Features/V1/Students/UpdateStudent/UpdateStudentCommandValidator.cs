using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;

internal sealed class UpdateStudentCommandValidator : ICommandValidator<UpdateStudentCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public UpdateStudentCommandValidator(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(UpdateStudentCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var student = await unitOfWork.StudentRepository.GetAsync(command.StudentId, cancellationToken);
        return student is null
            ? OtherError.NotFound()
            : CommandValidationResult.Ok();
    }
}
