using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;

internal sealed class CreateCourseFileHandler : ICommandHandler<CreateCourseFileCommand, CreateCourseFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ICommandValidator<CreateCourseFileCommand, CreateCourseFileCommandResponse> _validator;
    private readonly IClock _clock;

    public CreateCourseFileHandler(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        ICommandValidator<CreateCourseFileCommand, CreateCourseFileCommandResponse> validator,
        IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateCourseFileCommandResponse>> ExecuteAsync(
        CreateCourseFileCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var fileId = await unitOfWork.FileRepository.AddAsync(command.ToFileAddItem(_clock.GetCurrentTime()), cancellationToken);
        _ = await unitOfWork.CourseFileRepository.AddAsync(command.ToCourseFileAddItem(fileId), cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateCourseFileCommandResponse
        {
            FileId = fileId
        };
    }
}
