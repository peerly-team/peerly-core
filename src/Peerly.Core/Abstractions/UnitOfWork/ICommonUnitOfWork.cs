using Peerly.Core.Abstractions.Repositories;

namespace Peerly.Core.Abstractions.UnitOfWork;

public interface ICommonUnitOfWork : IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    IHomeworkRepository HomeworkRepository { get; }
    IGroupRepository GroupRepository { get; }
    IGroupStudentRepository GroupStudentRepository { get; }
    ICourseTeacherRepository CourseTeacherRepository { get; }
    IHomeworkSubmissionRepository HomeworkSubmissionRepository { get; }
}

public interface ICommonReadOnlyUnitOfWork : IUnitOfWork
{
    IReadOnlyCourseRepository ReadOnlyCourseRepository { get; }
    IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository { get; }
    IReadOnlyGroupRepository ReadOnlyGroupRepository { get; }
    IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository { get; }
    IReadOnlyCourseTeacherRepository ReadOnlyCourseTeacherRepository { get; }
    IReadOnlyHomeworkSubmissionRepository ReadOnlyHomeworkSubmissionRepository { get; }
}
