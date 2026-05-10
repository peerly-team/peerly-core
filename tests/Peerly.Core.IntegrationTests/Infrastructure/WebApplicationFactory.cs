using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.CreateCourseFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.DeleteCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.GetStudentCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.GetTeacherCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.PublishCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.SearchStudentCourses.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.SearchTeacherCourses.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Courses.UpdateCourse.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Groups.CreateGroup.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Homeworks.DeleteHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CorrectSubmittedHomeworkMark.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.CreateSubmittedReview.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomework.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Infrastructure;
using Peerly.Core.IntegrationTests.Features.V1.Submissions.DeleteSubmittedReview.Infrastructure;
using Peerly.Core.Messaging.Extensions;
using Peerly.Core.Persistence.Extensions;

namespace Peerly.Core.IntegrationTests.Infrastructure;

public sealed class WebApplicationFactory : IAsyncDisposable
{
    private readonly string _databaseHost;
    private readonly int _databasePort;
    private readonly string _databaseName;
    private readonly string _databaseUsername;
    private readonly string _databasePassword;
    private IHost? _host;
    private GrpcChannel? _channel;

    public WebApplicationFactory(
        string databaseHost,
        int databasePort,
        string databaseName,
        string databaseUsername,
        string databasePassword)
    {
        _databaseHost = databaseHost;
        _databasePort = databasePort;
        _databaseName = databaseName;
        _databaseUsername = databaseUsername;
        _databasePassword = databasePassword;
    }

    public CreateCourseGrpcClient CreateCourseClient()
    {
        return new CreateCourseGrpcClient(GetOrCreateGrpcChannel());
    }

    public CreateCourseFileGrpcClient CreateCourseFileClient()
    {
        return new CreateCourseFileGrpcClient(GetOrCreateGrpcChannel());
    }

    public GetStudentCourseGrpcClient GetStudentCourseClient()
    {
        return new GetStudentCourseGrpcClient(GetOrCreateGrpcChannel());
    }

    public GetTeacherCourseGrpcClient GetTeacherCourseClient()
    {
        return new GetTeacherCourseGrpcClient(GetOrCreateGrpcChannel());
    }

    public SearchStudentCoursesGrpcClient SearchStudentCoursesClient()
    {
        return new SearchStudentCoursesGrpcClient(GetOrCreateGrpcChannel());
    }

    public SearchTeacherCoursesGrpcClient SearchTeacherCoursesClient()
    {
        return new SearchTeacherCoursesGrpcClient(GetOrCreateGrpcChannel());
    }

    public UpdateCourseGrpcClient UpdateCourseClient()
    {
        return new UpdateCourseGrpcClient(GetOrCreateGrpcChannel());
    }

    public PublishCourseGrpcClient PublishCourseClient()
    {
        return new PublishCourseGrpcClient(GetOrCreateGrpcChannel());
    }

    public DeleteCourseGrpcClient DeleteCourseClient()
    {
        return new DeleteCourseGrpcClient(GetOrCreateGrpcChannel());
    }

    public CreateGroupGrpcClient CreateGroupClient()
    {
        return new CreateGroupGrpcClient(GetOrCreateGrpcChannel());
    }

    public DeleteHomeworkGrpcClient DeleteHomeworkClient()
    {
        return new DeleteHomeworkGrpcClient(GetOrCreateGrpcChannel());
    }

    public DeleteHomeworkFileGrpcClient DeleteHomeworkFileClient()
    {
        return new DeleteHomeworkFileGrpcClient(GetOrCreateGrpcChannel());
    }

    public CorrectSubmittedHomeworkMarkGrpcClient CorrectSubmittedHomeworkMarkClient()
    {
        return new CorrectSubmittedHomeworkMarkGrpcClient(GetOrCreateGrpcChannel());
    }

    public CreateSubmittedHomeworkGrpcClient CreateSubmittedHomeworkClient()
    {
        return new CreateSubmittedHomeworkGrpcClient(GetOrCreateGrpcChannel());
    }

    public CreateSubmittedHomeworkFileGrpcClient CreateSubmittedHomeworkFileClient()
    {
        return new CreateSubmittedHomeworkFileGrpcClient(GetOrCreateGrpcChannel());
    }

    public CreateSubmittedReviewGrpcClient CreateSubmittedReviewClient()
    {
        return new CreateSubmittedReviewGrpcClient(GetOrCreateGrpcChannel());
    }

    public DeleteSubmittedHomeworkGrpcClient DeleteSubmittedHomeworkClient()
    {
        return new DeleteSubmittedHomeworkGrpcClient(GetOrCreateGrpcChannel());
    }

    public DeleteSubmittedHomeworkFileGrpcClient DeleteSubmittedHomeworkFileClient()
    {
        return new DeleteSubmittedHomeworkFileGrpcClient(GetOrCreateGrpcChannel());
    }

    public DeleteSubmittedReviewGrpcClient DeleteSubmittedReviewClient()
    {
        return new DeleteSubmittedReviewGrpcClient(GetOrCreateGrpcChannel());
    }

    public async Task StartAsync()
    {
        _host = await Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                (_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(GetConfiguration());
                })
            .ConfigureWebHostDefaults(
                webBuilder =>
                {
                    webBuilder.UseEnvironment("IntegrationTests");
                    webBuilder.UseTestServer();
                    webBuilder.ConfigureServices(ConfigureServices);
                    webBuilder.Configure(ConfigureApplication);
                })
            .StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    private TestServer GetTestServer()
    {
        return _host?.GetTestServer()
            ?? throw new InvalidOperationException("Integration test host is not initialized.");
    }

    private GrpcChannel GetOrCreateGrpcChannel()
    {
        return _channel ??= GrpcChannel.ForAddress(
            "http://localhost",
            new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(GetTestServer().CreateHandler())
            });
    }

    private IReadOnlyDictionary<string, string?> GetConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionFactoryOptions:MasterHost"] = _databaseHost,
            ["ConnectionFactoryOptions:DefaultPort"] = _databasePort.ToString(),
            ["ConnectionFactoryOptions:Database"] = _databaseName,
            ["ConnectionFactoryOptions:UserName"] = _databaseUsername,
            ["ConnectionFactoryOptions:Password"] = _databasePassword,
            ["ConnectionFactoryOptions:SslMode"] = "Disable",
            ["ConnectionFactoryOptions:MaxPoolSize"] = "100",
            ["ConnectionFactoryOptions:ConnectionIdleLifetime"] = "300",
            ["KafkaConsumer:BootstrapServers"] = "localhost:9092",
            ["KafkaConsumer:GroupId"] = "peerly-core-tests",
            ["KafkaConsumer:Topic"] = "user_registration_events",
            ["KafkaConsumer:EnableAutoCommit"] = "false",
            ["KafkaConsumer:AutoOffsetReset"] = "earliest",
            ["Ceph:AccessKey"] = "test",
            ["Ceph:SecretKey"] = "test"
        };
    }

    private static void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
    {
        services.AddGrpc();
        services.AddGrpcReflection();

        services.ConfigureApi(context.Configuration);
        services.ConfigureApplicationServices(context.Configuration);
        services.ConfigureFileStorage(context.Configuration);
        services.ConfigureMessaging(context.Configuration);
        services.ConfigurePersistence(context.Configuration);

        RemoveHostedServices(services);
    }

    private static void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapGrpcService<StorageController>();
                endpoints.MapGrpcService<CourseController>();
                endpoints.MapGrpcService<HomeworkController>();
                endpoints.MapGrpcService<SubmissionController>();
                endpoints.MapGrpcService<GroupController>();
                endpoints.MapGrpcService<ParticipantController>();
                endpoints.MapGrpcReflectionService();
            });

        ValidationPropertyMappingConfiguration.Configure();
    }

    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
            .ToArray();

        foreach (var hostedService in hostedServices)
        {
            services.Remove(hostedService);
        }
    }
}
