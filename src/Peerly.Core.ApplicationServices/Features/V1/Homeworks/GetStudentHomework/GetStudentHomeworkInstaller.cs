using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

[ExcludeFromCodeCoverage]
internal sealed class GetStudentHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse>, GetStudentHomeworkQueryValidator>();
    }
}
