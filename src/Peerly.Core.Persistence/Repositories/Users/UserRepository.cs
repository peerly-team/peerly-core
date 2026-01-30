using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Persistence.UnitOfWork;

namespace Peerly.Core.Persistence.Repositories.Users;

internal sealed class UserRepository : IUserRepository
{
    private readonly IConnectionContext _connectionContext;

    public UserRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }


}
