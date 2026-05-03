using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;

[ExcludeFromCodeCoverage]
internal sealed class UpdateCourseHandlerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<UpdateCourseCommand, Success>, UpdateCourseCommandValidator>();
    }
}
