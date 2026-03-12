using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomework;

internal sealed class CreateHomeworkHandler : ICommandHandler<CreateHomeworkCommand, CreateHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public CreateHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateHomeworkCommandResponse>> ExecuteAsync(
        CreateHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        // todo: добавить проверку, что препод может добавлять домашку на курс
        // todo: добавить проверку, что курс и группа существуют

        var homeworkAddItem = new HomeworkAddItem
        {
            CourseId = command.CourseId,
            GroupId = command.GroupId,
            TeacherId = command.TeacherId,
            Name = command.Name,
            Status = HomeworkStatus.Draft,
            AmountOfReviewers = command.AmountOfReviewers,
            Description = command.Description,
            Checklist = command.Checklist,
            Deadline = command.Deadline,
            ReviewDeadline = command.ReviewDeadline,
            CreationTime = _clock.GetCurrentTime()
        };
        var homeworkId = await unitOfWork.HomeworkRepository.AddAsync(homeworkAddItem, cancellationToken);

        return new CreateHomeworkCommandResponse
        {
            HomeworkId = homeworkId
        };
    }
}
