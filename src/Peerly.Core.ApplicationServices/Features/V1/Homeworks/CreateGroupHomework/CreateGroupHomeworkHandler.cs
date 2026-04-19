using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;

internal sealed class CreateGroupHomeworkHandler : ICommandHandler<CreateGroupHomeworkCommand, CreateGroupHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICreateGroupHomeworkValidator _validator;
    private readonly IClock _clock;

    public CreateGroupHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        ICreateGroupHomeworkValidator validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateGroupHomeworkCommandResponse>> ExecuteAsync(
        CreateGroupHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var group = await unitOfWork.GroupRepository.GetAsync(command.GroupId, cancellationToken);
        if (group is null)
        {
            return OtherError.NotFound();
        }

        var validationError = await _validator.ValidateAsync(unitOfWork, command, group.CourseId, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var homeworkAddItem = command.ToHomeworkAddItem(group.CourseId, _clock.GetCurrentTime());
        var homeworkId = await unitOfWork.HomeworkRepository.AddAsync(homeworkAddItem, cancellationToken);

        return new CreateGroupHomeworkCommandResponse
        {
            HomeworkId = homeworkId
        };
    }
}
