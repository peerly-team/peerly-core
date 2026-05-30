using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;

[ExcludeFromCodeCoverage]
internal sealed class AddGroupTeacherInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<AddGroupTeacherCommand, Success>, AddGroupTeacherValidator>();
    }
}
