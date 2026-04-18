using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;

[ExcludeFromCodeCoverage]
internal sealed class PublishHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IPublishHomeworkHandlerMapper, PublishHomeworkHandlerMapper>();
    }
}
