using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;

[ExcludeFromCodeCoverage]
internal sealed class BulkAddGroupStudentsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<BulkAddGroupStudentsCommand, BulkAddGroupStudentsCommandResponse>, BulkAddGroupStudentsValidator>();
    }
}
