using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;

internal sealed class CreateCourseFileCommandValidator : ICommandValidator<CreateCourseFileCommand, CreateCourseFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public CreateCourseFileCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(CreateCourseFileCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseTeacher = command.ToCourseTeacher();
        var isCourseTeacherExists = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken);
        if (!isCourseTeacherExists)
        {
            return OtherError.PermissionDenied();
        }

        var isCourseExists = await unitOfWork.ReadOnlyCourseRepository.ExistsAsync(command.CourseId, cancellationToken);
        if (!isCourseExists)
        {
            return OtherError.NotFound(CourseErrors.CourseNotFound);
        }

        return CommandValidationResult.Ok();
    }
}
