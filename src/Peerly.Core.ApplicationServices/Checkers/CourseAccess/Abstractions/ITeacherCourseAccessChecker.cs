using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;

internal interface ITeacherCourseAccessChecker
{
    Task<bool> RunAsync(CourseTeacher courseTeacher, CancellationToken cancellationToken);
}
