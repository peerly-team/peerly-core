using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

internal sealed class DeleteSubmittedHomeworkFileValidator : IDeleteSubmittedHomeworkFileValidator
{
    public Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        DeleteSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: реализовать бизнес-валидации по образцу CreateSubmittedHomeworkFileValidator
        //       + HomeworkSubmissionAccessValidator:
        // 1. Existence SubmittedHomework: unitOfWork.SubmittedHomeworkRepository.GetAsync(...)
        //    → NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound)
        // 2. Ownership: submission.StudentId != command.StudentId
        //    → NotFound (скрываем PermissionDenied, не раскрываем существование чужой отправки)
        // 3. Existence файла в submitted_homework_files по (SubmittedHomeworkId, FileId)
        //    → NotFound (файл не принадлежит этой отправке). Потребуется ExistsAsync/GetAsync
        //    метод в ISubmittedHomeworkFileRepository.
        // 4. Access через IHomeworkSubmissionAccessValidator:
        //    - homework существует и не в Draft
        //    - IStudentCourseAccessChecker (студент в группе курса)
        //    - если homework групповой — студент в его группе
        //    - homework в статусе Published
        //    - deadline не истёк
        // 5. Conflict: если по этой отправке уже есть submitted_reviews/marks/distribution_reviewers
        //    → Conflict (нельзя менять состав файлов после начала рецензирования).
        return Task.FromResult<OtherError?>(null);
    }
}
