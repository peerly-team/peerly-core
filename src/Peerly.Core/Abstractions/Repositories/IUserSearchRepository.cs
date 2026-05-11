using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Users;

namespace Peerly.Core.Abstractions.Repositories;

public interface IReadOnlyUserSearchRepository
{
    Task<IReadOnlyCollection<User>> ListAsync(UserFilter filter, CancellationToken cancellationToken);
}
