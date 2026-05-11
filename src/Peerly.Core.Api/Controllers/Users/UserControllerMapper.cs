using System;
using Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;
using Peerly.Core.Models.Users;
using Peerly.Core.Tools;
using Peerly.Core.V1;
using Proto = Peerly.Core.V1.V1SearchUsersResponse.Types;

namespace Peerly.Core.Api.Controllers.Users;

internal static class UserControllerMapper
{
    public static SearchUsersQuery ToSearchUsersQuery(this V1SearchUsersRequest request)
    {
        return new SearchUsersQuery
        {
            Filter = new UserFilter
            {
                Query = request.Filter.Query,
                Roles = request.Filter.Roles.ToArrayBy(role => role.ToModel()),
                Limit = request.Limit
            }
        };
    }

    public static V1SearchUsersResponse ToV1SearchUsersResponse(this SearchUsersQueryResponse queryResponse)
    {
        return new V1SearchUsersResponse
        {
            Users = { queryResponse.Users.ToArrayBy(user => user.ToUserInfo()) }
        };
    }

    private static Proto.UserInfo ToUserInfo(this User user)
    {
        return new Proto.UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name ?? string.Empty,
            Role = user.Role.ToProto()
        };
    }

    private static UserRole ToModel(this Role roleProto)
    {
        return roleProto switch
        {
            Role.Teacher => UserRole.Teacher,
            Role.Student => UserRole.Student,
            _ => throw new ArgumentOutOfRangeException(nameof(roleProto), roleProto, null)
        };
    }

    private static Role ToProto(this UserRole role)
    {
        return role switch
        {
            UserRole.Teacher => Role.Teacher,
            UserRole.Student => Role.Student,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
}
