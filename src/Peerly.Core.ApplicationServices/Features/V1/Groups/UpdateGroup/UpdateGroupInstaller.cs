using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;

[ExcludeFromCodeCoverage]
internal sealed class UpdateGroupInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IUpdateGroupValidator, UpdateGroupValidator>();
    }
}
