using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;

[ExcludeFromCodeCoverage]
internal sealed class CreateGroupInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<CreateGroupCommand, CreateGroupCommandResponse>, CreateGroupCommandValidator>();
    }
}
