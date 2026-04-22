using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;

[ExcludeFromCodeCoverage]
internal sealed class DeleteSubmittedHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IDeleteSubmittedHomeworkValidator, DeleteSubmittedHomeworkValidator>();
    }
}
