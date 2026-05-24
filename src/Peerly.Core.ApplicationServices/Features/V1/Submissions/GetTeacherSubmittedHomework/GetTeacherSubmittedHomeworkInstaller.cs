using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

[ExcludeFromCodeCoverage]
internal sealed class GetTeacherSubmittedHomeworkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IQueryValidator<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse>, GetTeacherSubmittedHomeworkQueryValidator>();
    }
}
