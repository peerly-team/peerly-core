using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

[ExcludeFromCodeCoverage]
internal sealed class DeleteSubmittedHomeworkFileInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IDeleteSubmittedHomeworkFileValidator, DeleteSubmittedHomeworkFileValidator>();
    }
}
