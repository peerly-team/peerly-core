using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.Api.Controllers.Courses;
using Peerly.Core.Api.Controllers.Groups;
using Peerly.Core.Api.Controllers.Homeworks;
using Peerly.Core.Api.Controllers.Participants;
using Peerly.Core.Api.Controllers.Storage;
using Peerly.Core.Api.Controllers.Submissions;
using Peerly.Core.Api.Extensions;
using Peerly.Core.Api.Infrastructure.Configuration;
using Peerly.Core.ApplicationServices.Extensions;
using Peerly.Core.FileStorage.Extensions;
using Peerly.Core.Messaging.Extensions;
using Peerly.Core.Persistence.Extensions;

namespace Peerly.Core.Hosting;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureGrpc(builder);
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        RegistrationEndpoints(app);

        await app.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Api
        services.ConfigureApi(configuration);

        // ApplicationServices
        services.ConfigureApplicationServices(configuration);

        // FileStorage
        services.ConfigureFileStorage(configuration);

        // Messaging
        services.ConfigureMessaging(configuration);

        // Persistence
        services.ConfigurePersistence(configuration);
    }

    private static void ConfigureGrpc(WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();

        builder.WebHost.ConfigureKestrel(
            o =>
            {
                o.ListenLocalhost(
                    5001,
                    lo =>
                    {
                        lo.UseHttps();
                        lo.Protocols = HttpProtocols.Http2;
                    });
            });
    }

    private static void RegistrationEndpoints(WebApplication app)
    {
        app.UseRouting();

        app.MapGrpcService<StorageController>();
        app.MapGrpcService<CourseController>();
        app.MapGrpcService<HomeworkController>();
        app.MapGrpcService<SubmissionController>();
        app.MapGrpcService<GroupController>();
        app.MapGrpcService<ParticipantController>();

        app.MapGrpcReflectionService();

        // infrastructure configuration
        ValidationPropertyMappingConfiguration.Configure();
    }
}
