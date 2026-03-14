using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

internal sealed class CreateSubmittedHomeworkHandler :
    ICommandHandler<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public CreateSubmittedHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateSubmittedHomeworkCommandResponse>> ExecuteAsync(
        CreateSubmittedHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        // todo: добавить проверку, что студент находится на курсе/в группе, куда выложили эту домашку
        // todo: проверить, что домашка существует
        // todo: добавить проверку, что домашка в Published и now() < Deadline - ответы на домашку ещё принимаются

        var submittedHomeworkAddItem = new SubmittedHomeworkAddItem
        {
            HomeworkId = command.HomeworkId,
            StudentId = command.StudentId,
            Comment = command.Comment,
            CreationTime = _clock.GetCurrentTime()
        };
        var submittedHomeworkId = await unitOfWork.SubmittedHomeworkRepository.AddAsync(submittedHomeworkAddItem, cancellationToken);

        return new CreateSubmittedHomeworkCommandResponse
        {
            SubmittedHomeworkId = submittedHomeworkId
        };
    }
}
