using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

internal sealed class CreateCourseHandler : ICommandHandler<CreateCourseCommand, CreateCourseCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ICommandValidator<CreateCourseCommand> _validator;
    private readonly IClock _clock;

    public CreateCourseHandler(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IClock clock,
        ICommandValidator<CreateCourseCommand> validator)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
        _validator = validator;
    }

    public async Task<CommandResponse<CreateCourseCommandResponse>> ExecuteAsync(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var courseAddItem = command.ToCourseAddItem(_clock.GetCurrentTime());
        var courseId = await unitOfWork.CourseRepository.AddAsync(courseAddItem, cancellationToken);

        var courseTeacherAddItem = command.ToCourseTeacherAddItem(courseId, _clock.GetCurrentTime());
        _ = await unitOfWork.CourseTeacherRepository.AddAsync(courseTeacherAddItem, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateCourseCommandResponse
        {
            CourseId = courseId
        };
    }
}
