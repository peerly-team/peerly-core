using System.Diagnostics.CodeAnalysis;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.FileStorage.Configurations;

[ExcludeFromCodeCoverage]
internal sealed class FileStorageConfigurationsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        AWSConfigsS3.UseSignatureVersion4 = true;

        services.AddOptions<CephCredentials>()
            .BindConfiguration(CephCredentials.SectionName);

        services.AddOptions<CephOptions>()
            .BindConfiguration(CephOptions.SectionName);
    }
}
