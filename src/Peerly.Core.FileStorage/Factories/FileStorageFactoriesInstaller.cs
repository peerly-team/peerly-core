using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.FileStorage.Factories.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.FileStorage.Factories;

[ExcludeFromCodeCoverage]
internal sealed class FileStorageFactoriesInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddSingleton<IAmazonClientFactory, AmazonClientFactory>();
    }
}
