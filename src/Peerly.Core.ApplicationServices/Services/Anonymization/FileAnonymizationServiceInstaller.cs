using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Services.Anonymization.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Services.Anonymization;

[ExcludeFromCodeCoverage]
internal sealed class FileAnonymizationServiceInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IFileAnonymizationService, FileAnonymizationService>();
    }
}
