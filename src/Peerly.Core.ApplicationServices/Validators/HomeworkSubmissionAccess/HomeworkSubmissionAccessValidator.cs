using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.ApplicationServices.Validators.HomeworkSubmissionAccess.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Validators.HomeworkSubmissionAccess;

internal sealed class HomeworkSubmissionAccessValidator : IHomeworkSubmissionAccessValidator
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IStudentCourseAccessChecker _studentCourseAccessChecker;
    private readonly IClock _clock;

    public HomeworkSubmissionAccessValidator(
        IStudentCourseAccessChecker studentCourseAccessChecker,
        IClock clock,
        ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _studentCourseAccessChecker = studentCourseAccessChecker;
        _clock = clock;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<OtherError?> RunAsync(HomeworkId homeworkId, StudentId studentId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(homeworkId, cancellationToken);
        if (homework is null || homework.Status is HomeworkStatus.Draft)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        var accessError = await EnsureStudentHasAccessAsync(unitOfWork, studentId, homework, cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (homework.Status is not HomeworkStatus.Published)
        {
            return OtherError.Conflict(HomeworkErrors.HomeworkNotAcceptingSubmissions);
        }

        if (_clock.GetCurrentTime() >= homework.Deadline)
        {
            return OtherError.Conflict(HomeworkErrors.HomeworkDeadlinePassed);
        }

        return null;
    }

    private async Task<OtherError?> EnsureStudentHasAccessAsync(
        ICommonReadOnlyUnitOfWork unitOfWork,
        StudentId studentId,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var courseStudent = new CourseStudent
        {
            CourseId = homework.CourseId,
            StudentId = studentId
        };
        if (!await _studentCourseAccessChecker.RunAsync(courseStudent, cancellationToken))
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        if (homework.GroupId is not { } homeworkGroupId)
        {
            return null;
        }

        var groupStudent = new GroupStudent
        {
            GroupId = homeworkGroupId,
            StudentId = studentId
        };
        var isMember = await unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(groupStudent, cancellationToken);
        return isMember ? null : OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
    }
}
