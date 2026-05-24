using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;

[ExcludeFromCodeCoverage]
internal sealed class ListAssignedReviewsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<ListAssignedReviewsQuery, ListAssignedReviewsQueryResponse>, ListAssignedReviewsQueryValidator>();
    }
}
