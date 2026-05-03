using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;

internal sealed class UpdateCourseHandler : ICommandHandler<UpdateCourseCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<UpdateCourseCommand, Success> _validator;

    public UpdateCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, ICommandValidator<UpdateCourseCommand, Success> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        _ = await unitOfWork.CourseRepository.UpdateAsync(
            command.CourseId,
            builder => builder
                .Set(item => item.Name, command.Name)
                .Set(item => item.Description, command.Description),
            cancellationToken);

        return new Success();
    }
}
