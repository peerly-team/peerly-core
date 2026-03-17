using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

[ExcludeFromCodeCoverage]
internal sealed class CreateCourseHandlerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICreateCourseHandlerMapper, CreateCourseHandlerMapper>();
    }
}
