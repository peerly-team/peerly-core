using System;
using System.Data.Common;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;

namespace Peerly.Core.Persistence.UnitOfWork;

internal sealed class CommonUnitOfWork : UnitOfWork, ICommonUnitOfWork, ICommonReadOnlyUnitOfWork
{
    private readonly Lazy<ICourseRepository> _courseRepository;
    private readonly Lazy<IHomeworkRepository> _homeworkRepository;
    private readonly Lazy<IGroupRepository> _groupRepository;
    private readonly Lazy<IGroupStudentRepository> _groupStudentRepository;
    private readonly Lazy<ICourseTeacherRepository> _courseTeacherRepository;
    private readonly Lazy<ISubmittedHomeworkRepository> _submittedHomeworkRepository;
    private readonly Lazy<IFileRepository> _fileRepository;
    private readonly Lazy<IHomeworkFileRepository> _homeworkFileRepository;
    private readonly Lazy<ISubmittedHomeworkFileRepository> _submittedHomeworkFileRepository;

    public CommonUnitOfWork(
        DbConnection connection,
        Func<IConnectionContext, ICourseRepository> courseRepositoryFactory,
        Func<IConnectionContext, IHomeworkRepository> homeworkRepositoryFactory,
        Func<IConnectionContext, IGroupRepository> groupRepositoryFactory,
        Func<IConnectionContext, IGroupStudentRepository> groupStudentRepositoryFactory,
        Func<IConnectionContext, ICourseTeacherRepository> courseTeacherRepositoryFactory,
        Func<IConnectionContext, ISubmittedHomeworkRepository> submittedHomeworkRepositoryFactory,
        Func<IConnectionContext, IFileRepository> fileRepositoryFactory,
        Func<IConnectionContext, IHomeworkFileRepository> homeworkFileRepositoryFactory,
        Func<IConnectionContext, ISubmittedHomeworkFileRepository> submittedHomeworkFileRepositoryFactory) : base(connection)
    {
        _courseRepository = new Lazy<ICourseRepository>(() => courseRepositoryFactory(this));
        _homeworkRepository = new Lazy<IHomeworkRepository>(() => homeworkRepositoryFactory(this));
        _groupRepository = new Lazy<IGroupRepository>(() => groupRepositoryFactory(this));
        _groupStudentRepository = new Lazy<IGroupStudentRepository>(() => groupStudentRepositoryFactory(this));
        _courseTeacherRepository = new Lazy<ICourseTeacherRepository>(() => courseTeacherRepositoryFactory(this));
        _submittedHomeworkRepository = new Lazy<ISubmittedHomeworkRepository>(() => submittedHomeworkRepositoryFactory(this));
        _fileRepository = new Lazy<IFileRepository>(() => fileRepositoryFactory(this));
        _homeworkFileRepository = new Lazy<IHomeworkFileRepository>(() => homeworkFileRepositoryFactory(this));
        _submittedHomeworkFileRepository = new Lazy<ISubmittedHomeworkFileRepository>(() => submittedHomeworkFileRepositoryFactory(this));
    }

    public ICourseRepository CourseRepository => _courseRepository.Value;
    public IHomeworkRepository HomeworkRepository => _homeworkRepository.Value;
    public IGroupRepository GroupRepository => _groupRepository.Value;
    public IGroupStudentRepository GroupStudentRepository => _groupStudentRepository.Value;
    public ICourseTeacherRepository CourseTeacherRepository => _courseTeacherRepository.Value;
    public ISubmittedHomeworkRepository SubmittedHomeworkRepository => _submittedHomeworkRepository.Value;
    public IFileRepository FileRepository => _fileRepository.Value;
    public IHomeworkFileRepository HomeworkFileRepository => _homeworkFileRepository.Value;
    public ISubmittedHomeworkFileRepository SubmittedHomeworkFileRepository => _submittedHomeworkFileRepository.Value;

    public IReadOnlyCourseRepository ReadOnlyCourseRepository => _courseRepository.Value;
    public IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository => _homeworkRepository.Value;
    public IReadOnlyGroupRepository ReadOnlyGroupRepository => _groupRepository.Value;
    public IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository => _groupStudentRepository.Value;
    public IReadOnlyCourseTeacherRepository ReadOnlyCourseTeacherRepository => _courseTeacherRepository.Value;
    public IReadOnlySubmittedHomeworkRepository ReadOnlySubmittedHomeworkRepository => _submittedHomeworkRepository.Value;
    public IReadOnlyFileRepository ReadOnlyFileRepository => _fileRepository.Value;
    public IReadOnlyHomeworkFileRepository ReadOnlyHomeworkFileRepository => _homeworkFileRepository.Value;
    public IReadOnlySubmittedHomeworkFileRepository ReadOnlySubmittedHomeworkFileRepository => _submittedHomeworkFileRepository.Value;
}
