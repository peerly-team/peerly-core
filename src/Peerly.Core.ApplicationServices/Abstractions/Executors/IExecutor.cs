using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Core.ApplicationServices.Abstractions.Executors;

internal interface IExecutor<in TRequestItem>
{
    Task RunAsync(TRequestItem requestItem, CancellationToken cancellationToken);
}
