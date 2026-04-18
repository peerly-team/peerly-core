using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupTeacherRepository : IReadOnlyGroupTeacherRepository
{
}

public interface IReadOnlyGroupTeacherRepository
{
    Task<IReadOnlyCollection<TeacherId>> ListTeacherIdAsync(GroupId groupId, CancellationToken cancellationToken);
}
