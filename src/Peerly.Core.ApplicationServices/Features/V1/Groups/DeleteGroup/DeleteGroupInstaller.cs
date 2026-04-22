using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;

[ExcludeFromCodeCoverage]
internal sealed class DeleteGroupInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IDeleteGroupValidator, DeleteGroupValidator>();
    }
}
