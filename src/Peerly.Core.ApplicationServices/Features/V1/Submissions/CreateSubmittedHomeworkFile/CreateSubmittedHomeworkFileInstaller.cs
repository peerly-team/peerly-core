using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

[ExcludeFromCodeCoverage]
internal sealed class CreateSubmittedHomeworkFileInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse>, CreateSubmittedHomeworkFileValidator>();
    }
}
