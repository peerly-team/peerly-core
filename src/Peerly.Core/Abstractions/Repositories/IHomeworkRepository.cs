using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Pagination;

namespace Peerly.Core.Abstractions.Repositories;

public interface IHomeworkRepository : IReadOnlyHomeworkRepository
{
    Task<HomeworkId> AddAsync(HomeworkAddItem item, CancellationToken cancellationToken);
    Task DeleteAsync(HomeworkId homeworkId, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        HomeworkId homeworkId,
        Action<IUpdateBuilder<HomeworkUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyHomeworkRepository
{
    Task<Homework?> GetAsync(HomeworkId homeworkId, CancellationToken cancellationToken);
    Task<int> GetHomeworkCountAsync(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Homework>> ListAsync(HomeworkFilter filter, CancellationToken cancellationToken);

    Task<TeacherHomeworkInfo?> GetTeacherHomeworkInfoAsync(HomeworkId homeworkId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TeacherHomeworkInfo>> ListTeacherHomeworkInfosAsync(CourseTeacher courseTeacher, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TeacherHomeworkInfo>> SearchTeacherHomeworkInfosAsync(
        TeacherHomeworkSearchFilter filter,
        PaginationInfo paginationInfo,
        CancellationToken cancellationToken);

    Task<bool> ExistsByRubricIdAsync(RubricId rubricId, CancellationToken cancellationToken);
    Task<bool> ExistsPublishedByRubricIdAsync(RubricId rubricId, CancellationToken cancellationToken);

    Task<StudentHomeworkInfo?> GetStudentHomeworkInfoAsync(HomeworkStudent homeworkStudent, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StudentHomeworkInfo>> ListStudentHomeworkInfosAsync(CourseStudent courseStudent, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StudentHomeworkInfo>> SearchStudentHomeworkInfosAsync(
        StudentHomeworkSearchFilter filter,
        PaginationInfo paginationInfo,
        CancellationToken cancellationToken);
}
