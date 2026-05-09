using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

[ExcludeFromCodeCoverage]
internal sealed class CreateSubmittedHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<
            ICommandValidator<CreateSubmittedHomeworkCommand, CreateSubmittedHomeworkCommandResponse>,
            CreateSubmittedHomeworkCommandValidator>();
    }
}
