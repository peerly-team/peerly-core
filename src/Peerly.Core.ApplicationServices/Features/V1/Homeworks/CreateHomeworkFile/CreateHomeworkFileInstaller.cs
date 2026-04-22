using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;

[ExcludeFromCodeCoverage]
internal sealed class CreateHomeworkFileInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICreateHomeworkFileValidator, CreateHomeworkFileValidator>();
    }
}
