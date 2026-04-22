using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

internal sealed class CorrectSubmittedHomeworkMarkHandler : ICommandHandler<CorrectSubmittedHomeworkMarkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IClock _clock;

    public CorrectSubmittedHomeworkMarkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        CorrectSubmittedHomeworkMarkCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: permission — TeacherId должен иметь доступ к курсу, к которому относится
        // submittedHomework.HomeworkId. Паттерн: IReadOnlySubmittedHomeworkRepository.GetAsync ->
        // IReadOnlyHomeworkRepository.GetAsync -> ITeacherCourseAccessChecker.
        // Несоответствие → OtherError.NotFound() (скрытие существования сдачи).
        // Закрыть одним PR с ListSubmittedHomeworkOverview/GetTeacherSubmittedHomework.
        // TODO: статус homework'а — разрешить только Reviewing/Closed. Сейчас неявно покрыто через
        // UpdateAsync == false → NotFound, но явный статус даст OtherError.Conflict с понятной ошибкой.
        // TODO: has_discrepancy намеренно не пересчитываем — это инвариант расхождения между
        // рецензентами, фиксируется в ReviewCompletionJobExecutor.AggregateReviewersMarksAsync.
        // TODO: вынести в ICorrectSubmittedHomeworkMarkValidator + Validator + Installer при появлении
        // 2+ бизнес-проверок, см. feedback_validator_extraction.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var isSuccess = await unitOfWork.SubmittedHomeworkMarkRepository.UpdateAsync(
            command.SubmittedHomeworkId,
            builder => builder
                .Set(item => item.TeacherMark, command.TeacherMark)
                .Set(item => item.TeacherId, command.TeacherId)
                .Set(item => item.UpdateTime, _clock.GetCurrentTime()),
            cancellationToken);

        return isSuccess
            ? new Success()
            : OtherError.NotFound();
    }
}
