using System;
using Peerly.Core.Models.Users;
using Peerly.Core.Persistence.Repositories.UserSearch.Models;

namespace Peerly.Core.Persistence.Repositories.UserSearch;

internal static class UserSearchRepositoryMapper
{
    public static User ToUser(this UserDb db)
    {
        return new User
        {
            Id = db.Id,
            Email = db.Email,
            Name = db.Name,
            Role = Enum.Parse<UserRole>(db.Role)
        };
    }
}
