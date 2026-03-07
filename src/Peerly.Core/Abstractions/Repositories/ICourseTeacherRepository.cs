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
    Task<IReadOnlyCollection<CourseId>> ListCourseIdAsync(TeacherId teacherId, CancellationToken cancellationToken);
}
