using Peerly.Core.Abstractions.Repositories;

namespace Peerly.Core.Abstractions.UnitOfWork;

public interface ICommonUnitOfWork : IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ICourseRepository CourseRepository { get; }
    IHomeworkRepository HomeworkRepository { get; }
    IGroupRepository GroupRepository { get; }
    IGroupStudentRepository GroupStudentRepository { get; }
}

public interface ICommonReadOnlyUnitOfWork : IUnitOfWork
{
    IReadOnlyUserRepository ReadOnlyUserRepository { get; }
    IReadOnlyCourseRepository ReadOnlyCourseRepository { get; }
    IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository { get; }
    IReadOnlyGroupRepository ReadOnlyGroupRepository { get; }
    IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository { get; }
}
