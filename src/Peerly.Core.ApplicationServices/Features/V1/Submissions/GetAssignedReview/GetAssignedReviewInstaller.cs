using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;

[ExcludeFromCodeCoverage]
internal sealed class GetAssignedReviewInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<GetAssignedReviewQuery, GetAssignedReviewQueryResponse>, GetAssignedReviewQueryValidator>();
    }
}
