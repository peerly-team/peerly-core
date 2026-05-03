using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourseFile;

[ExcludeFromCodeCoverage]
internal sealed class CreateCourseFileInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<CreateCourseFileCommand, CreateCourseFileCommandResponse>, CreateCourseFileCommandValidator>();
    }
}
