using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

internal sealed class CreateCourseCommandValidator : ICommandValidator<CreateCourseCommand>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public CreateCourseCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var teacherFilter = command.ToTeacherFilter();
        var teachers = await unitOfWork.ReadOnlyTeacherRepository.ListAsync(teacherFilter, cancellationToken);

        return teachers.Count == teacherFilter.TeacherIds.Count
            ? CommandValidationResult.Ok()
            : OtherError.NotFound(TeacherErrors.TeacherNotFound);
    }
}
