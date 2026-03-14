using Peerly.Core.Abstractions.Repositories;

namespace Peerly.Core.Abstractions.UnitOfWork;

public interface ICommonUnitOfWork : IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    IHomeworkRepository HomeworkRepository { get; }
    IGroupRepository GroupRepository { get; }
    IGroupStudentRepository GroupStudentRepository { get; }
    ICourseTeacherRepository CourseTeacherRepository { get; }
    ISubmittedHomeworkRepository SubmittedHomeworkRepository { get; }
    IFileRepository FileRepository { get; }
    IHomeworkFileRepository HomeworkFileRepository { get; }
    ISubmittedHomeworkFileRepository SubmittedHomeworkFileRepository { get; }
}

public interface ICommonReadOnlyUnitOfWork : IUnitOfWork
{
    IReadOnlyCourseRepository ReadOnlyCourseRepository { get; }
    IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository { get; }
    IReadOnlyGroupRepository ReadOnlyGroupRepository { get; }
    IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository { get; }
    IReadOnlyCourseTeacherRepository ReadOnlyCourseTeacherRepository { get; }
    IReadOnlySubmittedHomeworkRepository ReadOnlySubmittedHomeworkRepository { get; }
    IReadOnlyFileRepository ReadOnlyFileRepository { get; }
    IReadOnlyHomeworkFileRepository ReadOnlyHomeworkFileRepository { get; }
    IReadOnlySubmittedHomeworkFileRepository ReadOnlySubmittedHomeworkFileRepository { get; }
}
