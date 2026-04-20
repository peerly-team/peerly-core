using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;

internal interface IStudentCourseAccessChecker
{
    Task<bool> RunAsync(CourseStudent courseStudent, CancellationToken cancellationToken);
}
