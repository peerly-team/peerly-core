using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.Persistence.Extensions;
using Peerly.Core.Persistence.Repositories.Courses;
using Peerly.Core.Persistence.Repositories.CourseTeachers;
using Peerly.Core.Persistence.Repositories.Files;
using Peerly.Core.Persistence.Repositories.Groups;
using Peerly.Core.Persistence.Repositories.GroupStudents;
using Peerly.Core.Persistence.Repositories.HomeworkFiles;
using Peerly.Core.Persistence.Repositories.Homeworks;
using Peerly.Core.Persistence.Repositories.SubmittedHomeworkFiles;
using Peerly.Core.Persistence.Repositories.HomeworkDistributions;
using Peerly.Core.Persistence.Repositories.SubmittedHomeworks;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.Persistence.UnitOfWork;

[ExcludeFromCodeCoverage]
public class UnitOfWorkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommonUnitOfWorkFactory, CommonUnitOfWorkFactory>();
        services.AddUnitOfWorkInnerFactory<CommonUnitOfWork>();
        services
            .AddOptions<ConnectionFactoryOptions>()
            .BindConfiguration(ConnectionFactoryOptions.SectionName);

        services.AddRepositoryFactory<ICourseRepository, CourseRepository>();
        services.AddRepositoryFactory<IHomeworkRepository, HomeworkRepository>();
        services.AddRepositoryFactory<IGroupRepository, GroupRepository>();
        services.AddRepositoryFactory<IGroupStudentRepository, GroupStudentRepository>();
        services.AddRepositoryFactory<ICourseTeacherRepository, CourseTeacherRepository>();
        services.AddRepositoryFactory<ISubmittedHomeworkRepository, SubmittedHomeworkRepository>();
        services.AddRepositoryFactory<IFileRepository, FileRepository>();
        services.AddRepositoryFactory<IHomeworkFileRepository, HomeworkFileRepository>();
        services.AddRepositoryFactory<ISubmittedHomeworkFileRepository, SubmittedHomeworkFileRepository>();
        services.AddRepositoryFactory<IHomeworkDistributionRepository, HomeworkDistributionRepository>();

        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<ConnectionFactoryOptions>>().Value;

            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = opt.MasterHost,
                Port = opt.DefaultPort,
                Database = opt.Database,
                Username = opt.UserName,
                Password = opt.Password,
                Pooling = true,
            };

            return NpgsqlDataSource.Create(csb.ConnectionString);
        });

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}
