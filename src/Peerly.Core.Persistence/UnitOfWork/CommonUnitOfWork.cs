using System;
using System.Data.Common;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;

namespace Peerly.Core.Persistence.UnitOfWork;

internal sealed class CommonUnitOfWork : UnitOfWork, ICommonUnitOfWork, ICommonReadOnlyUnitOfWork
{
    private readonly Lazy<IUserRepository> _userRepository;
    private readonly Lazy<ICourseRepository> _courseRepository;
    private readonly Lazy<IHomeworkRepository> _homeworkRepository;
    private readonly Lazy<IGroupRepository> _groupRepository;
    private readonly Lazy<IGroupStudentRepository> _groupStudentRepository;

    public CommonUnitOfWork(
        DbConnection connection,
        Func<IConnectionContext, IUserRepository> userRepositoryFactory,
        Func<IConnectionContext, ICourseRepository> courseRepositoryFactory,
        Func<IConnectionContext, IHomeworkRepository> homeworkRepositoryFactory,
        Func<IConnectionContext, IGroupRepository> groupRepositoryFactory,
        Func<IConnectionContext, IGroupStudentRepository> groupStudentRepositoryFactory) : base(connection)
    {
        _userRepository = new Lazy<IUserRepository>(() => userRepositoryFactory(this));
        _courseRepository = new Lazy<ICourseRepository>(() => courseRepositoryFactory(this));
        _homeworkRepository = new Lazy<IHomeworkRepository>(() => homeworkRepositoryFactory(this));
        _groupRepository = new Lazy<IGroupRepository>(() => groupRepositoryFactory(this));
        _groupStudentRepository = new Lazy<IGroupStudentRepository>(() => groupStudentRepositoryFactory(this));
    }

    public IUserRepository UserRepository => _userRepository.Value;
    public ICourseRepository CourseRepository => _courseRepository.Value;
    public IHomeworkRepository HomeworkRepository => _homeworkRepository.Value;
    public IGroupRepository GroupRepository => _groupRepository.Value;
    public IGroupStudentRepository GroupStudentRepository => _groupStudentRepository.Value;

    public IReadOnlyUserRepository ReadOnlyUserRepository => _userRepository.Value;
    public IReadOnlyCourseRepository ReadOnlyCourseRepository => _courseRepository.Value;
    public IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository => _homeworkRepository.Value;
    public IReadOnlyGroupRepository ReadOnlyGroupRepository => _groupRepository.Value;
    public IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository => _groupStudentRepository.Value;
}
