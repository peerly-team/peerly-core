using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;

[ExcludeFromCodeCoverage]
internal sealed class DeleteSubmittedReviewInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<DeleteSubmittedReviewCommand, Success>, DeleteSubmittedReviewCommandValidator>();
    }
}
