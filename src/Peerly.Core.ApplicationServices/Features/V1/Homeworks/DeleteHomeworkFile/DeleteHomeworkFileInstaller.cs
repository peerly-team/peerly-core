using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.DeleteHomeworkFile;

[ExcludeFromCodeCoverage]
internal sealed class DeleteHomeworkFileInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<DeleteHomeworkFileCommand, Success>, DeleteHomeworkFileCommandValidator>();
    }
}
