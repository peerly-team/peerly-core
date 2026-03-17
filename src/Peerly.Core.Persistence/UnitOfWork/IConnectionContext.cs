using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.Models.UnitOfWork;

namespace Peerly.Core.Persistence.UnitOfWork;

internal interface IConnectionContext
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    Task<IOperationSet> StartOperationSet(CancellationToken cancellationToken);
    Task<IOperationSet> StartOperationSet(TransactionRequirements requirements, CancellationToken cancellationToken);
}
