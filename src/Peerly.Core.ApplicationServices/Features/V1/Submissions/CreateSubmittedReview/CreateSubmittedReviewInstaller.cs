using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

[ExcludeFromCodeCoverage]
internal sealed class CreateSubmittedReviewInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICreateSubmittedReviewValidator, CreateSubmittedReviewValidator>();
    }
}
