using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;

[ExcludeFromCodeCoverage]
internal sealed class GetStudentRubricInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<GetStudentRubricQuery, GetStudentRubricQueryResponse>, GetStudentRubricQueryValidator>();
    }
}
