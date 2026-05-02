using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

internal sealed class GetTeacherCourseInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<GetTeacherCourseQuery, GetTeacherCourseQueryResponse>, GetTeacherCourseQueryValidator>();
    }
}
