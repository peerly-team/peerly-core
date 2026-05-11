using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Users;

namespace Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;

public sealed record SearchUsersQuery : IQuery<SearchUsersQueryResponse>
{
    public required UserFilter Filter { get; init; }
}
