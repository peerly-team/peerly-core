using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

[ExcludeFromCodeCoverage]
internal sealed class GetTeacherHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse>, GetTeacherHomeworkQueryValidator>();
    }
}
