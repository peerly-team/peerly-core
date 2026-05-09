using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CorrectSubmittedHomeworkMark;

[ExcludeFromCodeCoverage]
internal sealed class CorrectSubmittedHomeworkMarkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<CorrectSubmittedHomeworkMarkCommand, Success>, CorrectSubmittedHomeworkMarkCommandValidator>();

    }
}
