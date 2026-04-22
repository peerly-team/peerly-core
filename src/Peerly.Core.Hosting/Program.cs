using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        RegistrationEndpoints(app);

        await app.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpc();
        services.AddGrpcReflection();

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

    private static void RegistrationEndpoints(WebApplication app)
    {
        app.UseRouting();

        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

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
