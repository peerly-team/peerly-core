using System.Collections.Generic;
using Peerly.Core.Models.Users;

namespace Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;

public sealed record SearchUsersQueryResponse
{
    public required IReadOnlyCollection<User> Users { get; init; }
}
