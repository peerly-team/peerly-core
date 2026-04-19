using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;

[ExcludeFromCodeCoverage]
internal sealed class CreateCourseHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICreateCourseHomeworkValidator, CreateCourseHomeworkValidator>();
    }
}
