using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;

internal sealed class CreateCourseHomeworkHandler : ICommandHandler<CreateCourseHomeworkCommand, CreateCourseHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICreateCourseHomeworkValidator _validator;
    private readonly IClock _clock;

    public CreateCourseHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICreateCourseHomeworkValidator validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateCourseHomeworkCommandResponse>> ExecuteAsync(
        CreateCourseHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _validator.ValidateAsync(unitOfWork, command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var homeworkAddItem = command.ToHomeworkAddItem(_clock.GetCurrentTime());
        var homeworkId = await unitOfWork.HomeworkRepository.AddAsync(homeworkAddItem, cancellationToken);

        return new CreateCourseHomeworkCommandResponse
        {
            HomeworkId = homeworkId
        };
    }
}
