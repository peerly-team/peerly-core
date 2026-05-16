using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.Abstractions.Repositories;

public interface IGroupRepository : IReadOnlyGroupRepository
{
    Task<GroupId> AddAsync(GroupAddItem item, CancellationToken cancellationToken);
    Task DeleteAsync(GroupId groupId, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        GroupId groupId,
        Action<IUpdateBuilder<GroupUpdateItem>> configureUpdate,
        CancellationToken cancellationToken);
}

public interface IReadOnlyGroupRepository
{
    Task<Group?> GetAsync(GroupId groupId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(GroupId groupId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Group>> ListAsync(GroupFilter filter, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CourseId>> ListCourseIdsAsync(StudentId studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CourseId>> ListCourseIdAsync(TeacherId teacherId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GroupId>> ListGroupIdsAsync(StudentId studentId, CancellationToken cancellationToken);
}
