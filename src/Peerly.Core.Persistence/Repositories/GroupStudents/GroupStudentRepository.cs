using Peerly.Core.Abstractions.Repositories;
using Peerly.Core.Persistence.UnitOfWork;

namespace Peerly.Core.Persistence.Repositories.GroupStudents;

internal sealed class GroupStudentRepository : IGroupStudentRepository
{
    private readonly IConnectionContext _connectionContext;

    public GroupStudentRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }
}
