using System.Collections.Generic;

namespace Peerly.Core.Models.Users;

public sealed record UserFilter
{
    public required string Query { get; init; }
    public required IReadOnlyCollection<UserRole> Roles { get; init; }
    public required int Limit { get; init; }
}
