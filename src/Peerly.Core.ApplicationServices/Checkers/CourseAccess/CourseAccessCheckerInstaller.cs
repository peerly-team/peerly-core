using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Checkers.CourseAccess.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Checkers.CourseAccess;

internal sealed class CourseAccessCheckerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ITeacherCourseAccessChecker, TeacherCourseAccessChecker>();
    }
}
