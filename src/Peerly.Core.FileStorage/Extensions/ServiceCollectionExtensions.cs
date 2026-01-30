using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.Tools;

namespace Peerly.Core.FileStorage.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.InstallServicesFromExecutingAssembly(configuration);

        return services;
    }
}
