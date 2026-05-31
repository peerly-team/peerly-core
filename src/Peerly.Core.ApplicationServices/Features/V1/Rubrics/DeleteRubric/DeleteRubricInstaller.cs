using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;

[ExcludeFromCodeCoverage]
internal sealed class DeleteRubricInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<DeleteRubricCommand, Success>, DeleteRubricValidator>();
    }
}
