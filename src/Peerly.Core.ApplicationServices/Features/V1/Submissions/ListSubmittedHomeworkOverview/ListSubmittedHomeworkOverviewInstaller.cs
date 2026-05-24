using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;

[ExcludeFromCodeCoverage]
internal sealed class ListSubmittedHomeworkOverviewInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<ListSubmittedHomeworkOverviewQuery, ListSubmittedHomeworkOverviewQueryResponse>, ListSubmittedHomeworkOverviewQueryValidator>();
    }
}
