using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupStudentRepository : IReadOnlyGroupStudentRepository
{

}

public interface IReadOnlyGroupStudentRepository
{
    Task<IReadOnlyCollection<GroupId>> ListGroupIdAsync(StudentId studentId, CancellationToken cancellationToken);
}
