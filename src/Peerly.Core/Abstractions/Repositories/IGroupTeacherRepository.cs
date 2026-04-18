using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupTeacherRepository : IReadOnlyGroupTeacherRepository
{
    Task AddAsync(GroupTeacherAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyGroupTeacherRepository
{
    Task<IReadOnlyCollection<TeacherId>> ListTeacherIdAsync(GroupId groupId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GroupTeacher>> ListAsync(GroupTeacherFilter filter, CancellationToken cancellationToken);
}
