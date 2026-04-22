using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.ApplicationServices.Validators.HomeworkSubmissionAccess.Abstractions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.ApplicationServices.Validators.HomeworkSubmissionAccess;

[ExcludeFromCodeCoverage]
internal sealed class HomeworkSubmissionAccessValidatorInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IHomeworkSubmissionAccessValidator, HomeworkSubmissionAccessValidator>();
    }
}
