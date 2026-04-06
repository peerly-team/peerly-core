using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;

[ExcludeFromCodeCoverage]
internal sealed class UpdateDraftHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IUpdateDraftHomeworkValidator, UpdateDraftHomeworkValidator>();
    }
}
