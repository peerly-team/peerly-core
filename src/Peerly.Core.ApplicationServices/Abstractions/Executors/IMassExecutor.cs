using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Core.ApplicationServices.Abstractions.Executors;

internal interface IMassExecutor<in TRequestItem>
{
    Task RunAsync(IReadOnlyCollection<TRequestItem> requestItems, CancellationToken cancellationToken);
}
