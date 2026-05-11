using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;

internal sealed class SearchUsersHandler : IQueryHandler<SearchUsersQuery, SearchUsersQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public SearchUsersHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<SearchUsersQueryResponse> ExecuteAsync(SearchUsersQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var users = await unitOfWork.ReadOnlyUserSearchRepository.ListAsync(query.Filter, cancellationToken);

        return new SearchUsersQueryResponse { Users = users };
    }
}
