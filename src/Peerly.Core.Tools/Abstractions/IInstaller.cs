using Microsoft.Extensions.DependencyInjection;

namespace Peerly.Core.Tools.Abstractions;

public interface IInstaller
{
    void InstallServices(IServiceCollection services);
}
