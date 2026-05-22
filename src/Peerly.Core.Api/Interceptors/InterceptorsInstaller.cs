using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.Api.Interceptors.ServiceVersions;
using Peerly.Core.Api.Validators.Submissions;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.Api.Interceptors;

[ExcludeFromCodeCoverage]
internal sealed class InterceptorsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IServiceVersionContext, ServiceVersionContext>();

        services.AddGrpc(
            options =>
            {
                options.EnableDetailedErrors = true;
                options.Interceptors.Add<ExceptionInterceptor>();
                options.Interceptors.Add<FormatValidationInterceptor>();
                options.Interceptors.Add<ServiceVersionInterceptor>();
            });

        services.Scan(
            scan => scan
                .FromAssemblyOf<V1CreateSubmittedReviewRequestValidator>()
                .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
    }
}
