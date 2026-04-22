using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomeworkFile;

internal sealed class DeleteHomeworkFileHandler : ICommandHandler<DeleteHomeworkFileCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public DeleteHomeworkFileHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(DeleteHomeworkFileCommand command, CancellationToken cancellationToken)
    {
        // TODO: permission — TeacherId должен владеть курсом homework.CourseId
        // (через IReadOnlyHomeworkRepository.GetAsync(HomeworkId) → ITeacherCourseAccessChecker).
        // Несоответствие → OtherError.PermissionDenied() ДО проверки существования
        // (feedback_permission_before_existence).
        // TODO: статус — разрешить только HomeworkStatus.Draft. Для Published+ менять состав
        // файлов нельзя (они уже в submitted_homeworks-потоке) → OtherError.Conflict
        // или ValidationError.From(HomeworkErrors.IncorrectStatusForFileChange).
        // TODO: existence homework — GetAsync(HomeworkId) == null → OtherError.NotFound().
        // TODO: existence пары (HomeworkId, FileId) в homework_files — потребует ExistsAsync
        // метод в IReadOnlyHomeworkFileRepository. Сейчас DeleteAsync — silent no-op, клиент
        // получает Success даже для несуществующей пары.
        // TODO: S3 cleanup — перед удалением row'а получить StorageId из files → вызвать
        // IStorage.DeleteObjectAsync. Требует добавить DeleteObjectAsync в IStorage.
        // TODO: files-row cleanup — если это последнее упоминание file_id в homework_files
        // и submitted_homework_files, удалить row из files (shared pool). Отдельный refinement.
        // TODO: вынести в IDeleteHomeworkFileValidator + Validator + Installer при 2+ проверках,
        // см. feedback_validator_extraction.

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        await unitOfWork.HomeworkFileRepository.DeleteAsync(command.HomeworkId, command.FileId, cancellationToken);

        return new Success();
    }
}
