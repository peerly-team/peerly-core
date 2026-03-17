using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.FileStorage.Services;

[ExcludeFromCodeCoverage]
internal sealed class FileStorageServicesInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IFileImportService, FileImportService>();
        services.AddScoped<IStorage, CephStorage>();
    }
}
