using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;

internal sealed class ConfirmHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IConfirmHomeworkValidator, ConfirmHomeworkValidator>();
    }
}
