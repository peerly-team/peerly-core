using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupRepository : IReadOnlyGroupRepository
{

}

public interface IReadOnlyGroupRepository
{
    Task<IReadOnlyCollection<Group>> ListAsync(GroupFilter filter, CancellationToken cancellationToken);
}
