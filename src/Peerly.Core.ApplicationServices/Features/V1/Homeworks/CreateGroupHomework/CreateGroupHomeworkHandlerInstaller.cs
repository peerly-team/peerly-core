using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;

[ExcludeFromCodeCoverage]
internal sealed class CreateGroupHomeworkHandlerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICreateGroupHomeworkHandlerMapper, CreateGroupHomeworkHandlerMapper>();
    }
}
