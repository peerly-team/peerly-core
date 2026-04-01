using Peerly.Core.Abstractions.Repositories;

namespace Peerly.Core.Abstractions.UnitOfWork;

public interface ICommonUnitOfWork : IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    IDistributionReviewerRepository DistributionReviewerRepository { get; }
    IHomeworkRepository HomeworkRepository { get; }
    IGroupRepository GroupRepository { get; }
    IGroupStudentRepository GroupStudentRepository { get; }
    ICourseTeacherRepository CourseTeacherRepository { get; }
    ISubmittedHomeworkRepository SubmittedHomeworkRepository { get; }
    IFileRepository FileRepository { get; }
    IHomeworkFileRepository HomeworkFileRepository { get; }
    ISubmittedHomeworkFileRepository SubmittedHomeworkFileRepository { get; }
    IHomeworkDistributionRepository HomeworkDistributionRepository { get; }
    ISubmittedReviewRepository SubmittedReviewRepository { get; }
}

public interface ICommonReadOnlyUnitOfWork : IUnitOfWork
{
    IReadOnlyCourseRepository ReadOnlyCourseRepository { get; }
    IReadOnlyDistributionReviewerRepository ReadOnlyDistributionReviewerRepository { get; }
    IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository { get; }
    IReadOnlyGroupRepository ReadOnlyGroupRepository { get; }
    IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository { get; }
    IReadOnlyCourseTeacherRepository ReadOnlyCourseTeacherRepository { get; }
    IReadOnlySubmittedHomeworkRepository ReadOnlySubmittedHomeworkRepository { get; }
    IReadOnlyFileRepository ReadOnlyFileRepository { get; }
    IReadOnlyHomeworkFileRepository ReadOnlyHomeworkFileRepository { get; }
    IReadOnlySubmittedHomeworkFileRepository ReadOnlySubmittedHomeworkFileRepository { get; }
    IReadOnlyHomeworkDistributionRepository ReadOnlyHomeworkDistributionRepository { get; }
    IReadOnlySubmittedReviewRepository ReadOnlySubmittedReviewRepository { get; }
}
