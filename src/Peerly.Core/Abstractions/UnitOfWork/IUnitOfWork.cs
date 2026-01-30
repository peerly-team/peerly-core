using System;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Models.UnitOfWork;

namespace Peerly.Core.Abstractions.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<IOperationSet> StartOperationSet(CancellationToken cancellationToken);
    Task<IOperationSet> StartOperationSet(TransactionRequirements requirements, CancellationToken cancellationToken);
}
