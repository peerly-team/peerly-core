using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;

internal sealed class DeleteCourseHandler : ICommandHandler<DeleteCourseCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICommandValidator<DeleteCourseCommand> _validator;

    public DeleteCourseHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, ICommandValidator<DeleteCourseCommand> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        _ = await unitOfWork.CourseRepository.UpdateAsync(
            command.CourseId,
            builder => builder.Set(item => item.Status, CourseStatus.Deleted),
            cancellationToken);

        return new Success();
    }
}
