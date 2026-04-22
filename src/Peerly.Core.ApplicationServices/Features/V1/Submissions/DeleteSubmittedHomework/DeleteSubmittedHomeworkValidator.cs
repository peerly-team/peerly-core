using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;

internal sealed class DeleteSubmittedHomeworkValidator : IDeleteSubmittedHomeworkValidator
{
    public Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        DeleteSubmittedHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: реализовать бизнес-валидации по образцу UpdateSubmittedHomeworkValidator
        //       + HomeworkSubmissionAccessValidator:
        // 1. Existence: unitOfWork.SubmittedHomeworkRepository.GetAsync(...)
        //    → NotFound(SubmittedHomeworkErrors.SubmittedHomeworkNotFound)
        // 2. Ownership: submission.StudentId != command.StudentId
        //    → NotFound (скрываем PermissionDenied, не раскрываем существование чужой отправки)
        // 3. Access через IHomeworkSubmissionAccessValidator:
        //    - homework существует и не в Draft
        //    - IStudentCourseAccessChecker (студент в группе курса)
        //    - если homework групповой — студент в его группе
        //    - homework в статусе Published
        //    - deadline не истёк
        // 4. Conflict: если по submitted_homework_id есть записи в
        //    submitted_reviews / submitted_homework_marks / distribution_reviewers
        //    → Conflict (потребует HasAnyAsync методов и нового ErrorMessage).
        return Task.FromResult<OtherError?>(null);
    }
}
