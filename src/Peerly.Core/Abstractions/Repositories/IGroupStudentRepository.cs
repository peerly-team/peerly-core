using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupStudentRepository : IReadOnlyGroupStudentRepository
{

}

public interface IReadOnlyGroupStudentRepository
{
    Task<IReadOnlyCollection<GroupStudent>> ListAsync(GroupStudentFilter filter, CancellationToken cancellationToken);
}
