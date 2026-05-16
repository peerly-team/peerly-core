using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.Abstractions.Repositories;

public interface ICourseTeacherRepository : IReadOnlyCourseTeacherRepository
{
    Task<bool> AddAsync(CourseTeacherAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyCourseTeacherRepository
{
    Task<IReadOnlyCollection<CourseId>> ListCourseIdsAsync(TeacherId teacherId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TeacherId>> ListTeacherIdsAsync(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CourseTeacher>> ListAsync(IReadOnlyCollection<CourseId> courseIds, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(CourseTeacher courseTeacher, CancellationToken cancellationToken);
}
