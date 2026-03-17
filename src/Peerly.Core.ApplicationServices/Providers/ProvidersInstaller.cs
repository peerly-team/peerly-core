using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Providers;

[ExcludeFromCodeCoverage]
internal sealed class ProvidersInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IClock, Clock>();
    }
}
