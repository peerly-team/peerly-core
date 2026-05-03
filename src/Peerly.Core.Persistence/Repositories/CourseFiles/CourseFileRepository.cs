using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Files;
using Peerly.Core.Persistence.Repositories.Files;
using Peerly.Core.Persistence.Repositories.Files.Models;
using Peerly.Core.Persistence.UnitOfWork;
using Peerly.Core.Tools;
using static Peerly.Core.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Core.Persistence.Repositories.CourseFiles;

internal sealed class CourseFileRepository : ICourseFileRepository
{
    private readonly IConnectionContext _connectionContext;

    public CourseFileRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<bool> AddAsync(CourseFileAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)item.CourseId,
            FileId = (long)item.FileId,
            TeacherId = (long)item.TeacherId
        };

        const string Query =
            $"""
             insert into {CourseFileTable.TableName} (
                         {CourseFileTable.CourseId},
                         {CourseFileTable.FileId},
                         {CourseFileTable.TeacherId})
                  values (
                         @{nameof(queryParams.CourseId)},
                         @{nameof(queryParams.FileId)},
                         @{nameof(queryParams.TeacherId)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }

    public async Task<IReadOnlyCollection<File>> ListFilesAsync(CourseId courseId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            CourseId = (long)courseId
        };

        const string Query =
            $"""
             select f.{FileTable.Id},
                    f.{FileTable.StorageId},
                    f.{FileTable.Name},
                    f.{FileTable.Size}
               from {CourseFileTable.TableName} cf
               join {FileTable.TableName} f on f.{FileTable.Id} = cf.{CourseFileTable.FileId}
              where cf.{CourseFileTable.CourseId} = @{nameof(queryParams.CourseId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<FileDb>(command);

        return dbs.ToArrayBy(db => db.ToFile());
    }
}
