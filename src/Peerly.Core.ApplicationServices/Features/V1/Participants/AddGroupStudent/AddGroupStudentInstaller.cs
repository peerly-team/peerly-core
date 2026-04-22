using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;

[ExcludeFromCodeCoverage]
internal sealed class AddGroupStudentInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IAddGroupStudentValidator, AddGroupStudentValidator>();
    }
}
