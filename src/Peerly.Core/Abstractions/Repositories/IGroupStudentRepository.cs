using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupStudentRepository : IReadOnlyGroupStudentRepository
{
    Task AddAsync(GroupStudentAddItem item, CancellationToken cancellationToken);
    Task DeleteByGroupAsync(GroupId groupId, CancellationToken cancellationToken);
}

public interface IReadOnlyGroupStudentRepository
{
    Task<bool> ExistsAsync(GroupStudent groupStudent, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(CourseStudent courseStudent, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GroupStudent>> ListAsync(GroupStudentFilter filter, CancellationToken cancellationToken);
}
