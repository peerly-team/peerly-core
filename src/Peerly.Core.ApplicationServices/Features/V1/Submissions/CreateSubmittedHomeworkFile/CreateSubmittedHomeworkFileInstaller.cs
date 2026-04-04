using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

[ExcludeFromCodeCoverage]
internal sealed class CreateSubmittedHomeworkFileInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICreateSubmittedHomeworkFileHandlerMapper, CreateSubmittedHomeworkFileHandlerMapper>();
    }
}
