using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;

[ExcludeFromCodeCoverage]
internal sealed class PostponeHomeworkDeadlinesInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IPostponeHomeworkDeadlinesValidator, PostponeHomeworkDeadlinesValidator>();
    }
}
