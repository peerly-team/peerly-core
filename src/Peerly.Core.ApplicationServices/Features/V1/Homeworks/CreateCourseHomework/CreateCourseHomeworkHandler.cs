using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;

internal sealed class CreateCourseHomeworkHandler : ICommandHandler<CreateCourseHomeworkCommand, CreateCourseHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public CreateCourseHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateCourseHomeworkCommandResponse>> ExecuteAsync(
        CreateCourseHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        // todo: добавить проверку, что курс существуют
        // todo: добавить проверку, что препод может добавлять домашку на курс

        var homeworkAddItem = new HomeworkAddItem
        {
            CourseId = command.CourseId,
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

        return new CreateCourseHomeworkCommandResponse
        {
            HomeworkId = homeworkId
        };
    }
}
