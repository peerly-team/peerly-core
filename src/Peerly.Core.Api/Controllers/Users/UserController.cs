using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Users;

[ExcludeFromCodeCoverage]
public sealed class UserController : UserService.UserServiceBase
{
    private readonly IQueryHandler<SearchUsersQuery, SearchUsersQueryResponse> _searchUsersHandler;

    public UserController(IQueryHandler<SearchUsersQuery, SearchUsersQueryResponse> searchUsersHandler)
    {
        _searchUsersHandler = searchUsersHandler;
    }

    public override async Task<V1SearchUsersResponse> V1SearchUsers(V1SearchUsersRequest request, ServerCallContext context)
    {
        var query = request.ToSearchUsersQuery();
        var queryResponse = await _searchUsersHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchUsersResponse();
    }
}
