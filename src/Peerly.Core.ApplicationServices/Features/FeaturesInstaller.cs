using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.Storage.GenerateUploadUrl;
using Peerly.Core.Tools;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features;

[ExcludeFromCodeCoverage]
internal sealed class FeaturesInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.Scan(
            scan => scan
                .FromAssemblyOf<GenerateUploadUrlHandler>()
                .AddNonGenericImplementationsOf(typeof(IQueryHandler<,>))
                .WithScopedLifetime());
    }
}
