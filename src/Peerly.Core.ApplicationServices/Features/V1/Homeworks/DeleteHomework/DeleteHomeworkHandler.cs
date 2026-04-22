using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomework;

internal sealed class DeleteHomeworkHandler : ICommandHandler<DeleteHomeworkCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public DeleteHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(
        DeleteHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: permission — TeacherId должен владеть курсом homework.CourseId
        // (через ITeacherCourseAccessChecker + предварительный GetAsync(HomeworkId)).
        // Несоответствие → OtherError.PermissionDenied() ДО проверки существования
        // (feedback_permission_before_existence).
        // TODO: статус — разрешить только HomeworkStatus.Draft. Non-Draft →
        // ValidationError.From(HomeworkErrors.IncorrectStatusForDelete). Этот invariant
        // гарантирует, что handler не должен удалять submitted_homeworks и их детей.
        // TODO: каскад для non-Draft (после разблокировки Published+):
        //   submitted_homework_files → submitted_reviews → submitted_homework_marks →
        //   distribution_reviewers → submitted_homeworks → review_completions →
        //   homework_distributions → homework_files → homeworks.
        //   Потребует добавить DeleteByHomeworkAsync в соответствующие репозитории.
        // TODO: S3 cleanup — перед удалением homework_files row'ов получить file_ids →
        // files → вызвать IStorage.DeleteObjectAsync. Требует добавить DeleteObjectAsync
        // в IStorage + реализацию в CephStorage.
        // TODO: background jobs — при non-Draft удалить homework_distributions и
        // review_completions перед homework, иначе HomeworkDistributionJob/ReviewCompletionJob
        // будут падать на отсутствующем homework.
        // TODO: вынести в IDeleteHomeworkValidator + Validator + Installer при 2+ проверках,
        // см. feedback_validator_extraction.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);
        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        await unitOfWork.HomeworkFileRepository.DeleteByHomeworkAsync(command.HomeworkId, cancellationToken);
        await unitOfWork.HomeworkRepository.DeleteAsync(command.HomeworkId, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new Success();
    }
}
