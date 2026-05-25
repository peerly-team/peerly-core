using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Teachers.UpdateTeacher;

internal sealed class UpdateTeacherCommandValidator : ICommandValidator<UpdateTeacherCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public UpdateTeacherCommandValidator(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(UpdateTeacherCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var teacher = await unitOfWork.TeacherRepository.GetAsync(command.TeacherId, cancellationToken);
        return teacher is null
            ? OtherError.NotFound()
            : CommandValidationResult.Ok();
    }
}
