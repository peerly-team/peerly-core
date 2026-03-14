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
    private readonly Lazy<IHomeworkSubmissionRepository> _homeworkSubmissionRepository;
    private readonly Lazy<IFileRepository> _fileRepository;

    public CommonUnitOfWork(
        DbConnection connection,
        Func<IConnectionContext, ICourseRepository> courseRepositoryFactory,
        Func<IConnectionContext, IHomeworkRepository> homeworkRepositoryFactory,
        Func<IConnectionContext, IGroupRepository> groupRepositoryFactory,
        Func<IConnectionContext, IGroupStudentRepository> groupStudentRepositoryFactory,
        Func<IConnectionContext, ICourseTeacherRepository> courseTeacherRepositoryFactory,
        Func<IConnectionContext, IHomeworkSubmissionRepository> homeworkSubmissionRepositoryFactory,
        Func<IConnectionContext, IFileRepository> fileRepositoryFactory) : base(connection)
    {
        _courseRepository = new Lazy<ICourseRepository>(() => courseRepositoryFactory(this));
        _homeworkRepository = new Lazy<IHomeworkRepository>(() => homeworkRepositoryFactory(this));
        _groupRepository = new Lazy<IGroupRepository>(() => groupRepositoryFactory(this));
        _groupStudentRepository = new Lazy<IGroupStudentRepository>(() => groupStudentRepositoryFactory(this));
        _courseTeacherRepository = new Lazy<ICourseTeacherRepository>(() => courseTeacherRepositoryFactory(this));
        _homeworkSubmissionRepository = new Lazy<IHomeworkSubmissionRepository>(() => homeworkSubmissionRepositoryFactory(this));
        _fileRepository = new Lazy<IFileRepository>(() => fileRepositoryFactory(this));
    }

    public ICourseRepository CourseRepository => _courseRepository.Value;
    public IHomeworkRepository HomeworkRepository => _homeworkRepository.Value;
    public IGroupRepository GroupRepository => _groupRepository.Value;
    public IGroupStudentRepository GroupStudentRepository => _groupStudentRepository.Value;
    public ICourseTeacherRepository CourseTeacherRepository => _courseTeacherRepository.Value;
    public IHomeworkSubmissionRepository HomeworkSubmissionRepository => _homeworkSubmissionRepository.Value;
    public IFileRepository FileRepository => _fileRepository.Value;

    public IReadOnlyCourseRepository ReadOnlyCourseRepository => _courseRepository.Value;
    public IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository => _homeworkRepository.Value;
    public IReadOnlyGroupRepository ReadOnlyGroupRepository => _groupRepository.Value;
    public IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository => _groupStudentRepository.Value;
    public IReadOnlyCourseTeacherRepository ReadOnlyCourseTeacherRepository => _courseTeacherRepository.Value;
    public IReadOnlyHomeworkSubmissionRepository ReadOnlyHomeworkSubmissionRepository => _homeworkSubmissionRepository.Value;
    public IReadOnlyFileRepository ReadOnlyFileRepository => _fileRepository.Value;
}
