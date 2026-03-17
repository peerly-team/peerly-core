using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Peerly.Core.Tools.Abstractions;

public interface IConfigurableInstaller
{
    void InstallServices(IServiceCollection services, IConfiguration configuration);
}
