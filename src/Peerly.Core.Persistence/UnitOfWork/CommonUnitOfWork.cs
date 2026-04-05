using System;
using System.Data.Common;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;

namespace Peerly.Core.Persistence.UnitOfWork;

internal sealed class CommonUnitOfWork : UnitOfWork, ICommonUnitOfWork, ICommonReadOnlyUnitOfWork
{
    private readonly Lazy<ICourseRepository> _courseRepository;
    private readonly Lazy<IDistributionReviewerRepository> _distributionReviewerRepository;
    private readonly Lazy<IHomeworkRepository> _homeworkRepository;
    private readonly Lazy<IGroupRepository> _groupRepository;
    private readonly Lazy<IGroupStudentRepository> _groupStudentRepository;
    private readonly Lazy<ICourseTeacherRepository> _courseTeacherRepository;
    private readonly Lazy<ISubmittedHomeworkRepository> _submittedHomeworkRepository;
    private readonly Lazy<IFileRepository> _fileRepository;
    private readonly Lazy<IHomeworkFileRepository> _homeworkFileRepository;
    private readonly Lazy<ISubmittedHomeworkFileRepository> _submittedHomeworkFileRepository;
    private readonly Lazy<IHomeworkDistributionRepository> _homeworkDistributionRepository;
    private readonly Lazy<IReviewCompletionRepository> _reviewCompletionRepository;
    private readonly Lazy<ISubmittedReviewRepository> _submittedReviewRepository;
    private readonly Lazy<ISubmittedHomeworkMarkRepository> _submittedHomeworkMarkRepository;
    private readonly Lazy<IStudentRepository> _studentRepository;
    private readonly Lazy<ITeacherRepository> _teacherRepository;

    public CommonUnitOfWork(
        DbConnection connection,
        Func<IConnectionContext, ICourseRepository> courseRepositoryFactory,
        Func<IConnectionContext, IDistributionReviewerRepository> distributionReviewerRepositoryFactory,
        Func<IConnectionContext, IHomeworkRepository> homeworkRepositoryFactory,
        Func<IConnectionContext, IGroupRepository> groupRepositoryFactory,
        Func<IConnectionContext, IGroupStudentRepository> groupStudentRepositoryFactory,
        Func<IConnectionContext, ICourseTeacherRepository> courseTeacherRepositoryFactory,
        Func<IConnectionContext, ISubmittedHomeworkRepository> submittedHomeworkRepositoryFactory,
        Func<IConnectionContext, IFileRepository> fileRepositoryFactory,
        Func<IConnectionContext, IHomeworkFileRepository> homeworkFileRepositoryFactory,
        Func<IConnectionContext, ISubmittedHomeworkFileRepository> submittedHomeworkFileRepositoryFactory,
        Func<IConnectionContext, IHomeworkDistributionRepository> homeworkDistributionRepositoryFactory,
        Func<IConnectionContext, IReviewCompletionRepository> reviewCompletionRepositoryFactory,
        Func<IConnectionContext, ISubmittedReviewRepository> submittedReviewRepositoryFactory,
        Func<IConnectionContext, ISubmittedHomeworkMarkRepository> submittedHomeworkMarkRepositoryFactory,
        Func<IConnectionContext, IStudentRepository> studentRepositoryFactory,
        Func<IConnectionContext, ITeacherRepository> teacherRepositoryFactory) : base(connection)
    {
        _courseRepository = new Lazy<ICourseRepository>(() => courseRepositoryFactory(this));
        _distributionReviewerRepository = new Lazy<IDistributionReviewerRepository>(() => distributionReviewerRepositoryFactory(this));
        _homeworkRepository = new Lazy<IHomeworkRepository>(() => homeworkRepositoryFactory(this));
        _groupRepository = new Lazy<IGroupRepository>(() => groupRepositoryFactory(this));
        _groupStudentRepository = new Lazy<IGroupStudentRepository>(() => groupStudentRepositoryFactory(this));
        _courseTeacherRepository = new Lazy<ICourseTeacherRepository>(() => courseTeacherRepositoryFactory(this));
        _submittedHomeworkRepository = new Lazy<ISubmittedHomeworkRepository>(() => submittedHomeworkRepositoryFactory(this));
        _fileRepository = new Lazy<IFileRepository>(() => fileRepositoryFactory(this));
        _homeworkFileRepository = new Lazy<IHomeworkFileRepository>(() => homeworkFileRepositoryFactory(this));
        _submittedHomeworkFileRepository = new Lazy<ISubmittedHomeworkFileRepository>(() => submittedHomeworkFileRepositoryFactory(this));
        _homeworkDistributionRepository = new Lazy<IHomeworkDistributionRepository>(() => homeworkDistributionRepositoryFactory(this));
        _reviewCompletionRepository = new Lazy<IReviewCompletionRepository>(() => reviewCompletionRepositoryFactory(this));
        _submittedReviewRepository = new Lazy<ISubmittedReviewRepository>(() => submittedReviewRepositoryFactory(this));
        _submittedHomeworkMarkRepository = new Lazy<ISubmittedHomeworkMarkRepository>(() => submittedHomeworkMarkRepositoryFactory(this));
        _studentRepository = new Lazy<IStudentRepository>(() => studentRepositoryFactory(this));
        _teacherRepository = new Lazy<ITeacherRepository>(() => teacherRepositoryFactory(this));
    }

    public ICourseRepository CourseRepository => _courseRepository.Value;
    public IDistributionReviewerRepository DistributionReviewerRepository => _distributionReviewerRepository.Value;
    public IHomeworkRepository HomeworkRepository => _homeworkRepository.Value;
    public IGroupRepository GroupRepository => _groupRepository.Value;
    public IGroupStudentRepository GroupStudentRepository => _groupStudentRepository.Value;
    public ICourseTeacherRepository CourseTeacherRepository => _courseTeacherRepository.Value;
    public ISubmittedHomeworkRepository SubmittedHomeworkRepository => _submittedHomeworkRepository.Value;
    public IFileRepository FileRepository => _fileRepository.Value;
    public IHomeworkFileRepository HomeworkFileRepository => _homeworkFileRepository.Value;
    public ISubmittedHomeworkFileRepository SubmittedHomeworkFileRepository => _submittedHomeworkFileRepository.Value;
    public IHomeworkDistributionRepository HomeworkDistributionRepository => _homeworkDistributionRepository.Value;
    public IReviewCompletionRepository ReviewCompletionRepository => _reviewCompletionRepository.Value;
    public ISubmittedReviewRepository SubmittedReviewRepository => _submittedReviewRepository.Value;
    public ISubmittedHomeworkMarkRepository SubmittedHomeworkMarkRepository => _submittedHomeworkMarkRepository.Value;
    public IStudentRepository StudentRepository => _studentRepository.Value;
    public ITeacherRepository TeacherRepository => _teacherRepository.Value;

    public IReadOnlyCourseRepository ReadOnlyCourseRepository => _courseRepository.Value;
    public IReadOnlyDistributionReviewerRepository ReadOnlyDistributionReviewerRepository => _distributionReviewerRepository.Value;
    public IReadOnlyHomeworkRepository ReadOnlyHomeworkRepository => _homeworkRepository.Value;
    public IReadOnlyGroupRepository ReadOnlyGroupRepository => _groupRepository.Value;
    public IReadOnlyGroupStudentRepository ReadOnlyGroupStudentRepository => _groupStudentRepository.Value;
    public IReadOnlyCourseTeacherRepository ReadOnlyCourseTeacherRepository => _courseTeacherRepository.Value;
    public IReadOnlySubmittedHomeworkRepository ReadOnlySubmittedHomeworkRepository => _submittedHomeworkRepository.Value;
    public IReadOnlyFileRepository ReadOnlyFileRepository => _fileRepository.Value;
    public IReadOnlyHomeworkFileRepository ReadOnlyHomeworkFileRepository => _homeworkFileRepository.Value;
    public IReadOnlySubmittedHomeworkFileRepository ReadOnlySubmittedHomeworkFileRepository => _submittedHomeworkFileRepository.Value;
    public IReadOnlyHomeworkDistributionRepository ReadOnlyHomeworkDistributionRepository => _homeworkDistributionRepository.Value;
    public IReadOnlyReviewCompletionRepository ReadOnlyReviewCompletionRepository => _reviewCompletionRepository.Value;
    public IReadOnlySubmittedReviewRepository ReadOnlySubmittedReviewRepository => _submittedReviewRepository.Value;
    public IReadOnlySubmittedHomeworkMarkRepository ReadOnlySubmittedHomeworkMarkRepository => _submittedHomeworkMarkRepository.Value;
    public IReadOnlyStudentRepository ReadOnlyStudentRepository => _studentRepository.Value;
    public IReadOnlyTeacherRepository ReadOnlyTeacherRepository => _teacherRepository.Value;
}
