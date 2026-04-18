using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupRepository : IReadOnlyGroupRepository
{
    Task<GroupId> AddAsync(GroupAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyGroupRepository
{
    Task<Group?> GetAsync(GroupId groupId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(GroupId groupId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Group>> ListAsync(GroupFilter filter, CancellationToken cancellationToken);
}
