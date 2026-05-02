using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

[ExcludeFromCodeCoverage]
internal sealed class GetStudentCourseInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<GetStudentCourseQuery>, GetStudentCourseQueryValidator>();
    }
}
