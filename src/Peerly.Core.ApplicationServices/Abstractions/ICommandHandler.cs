using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Abstractions;

public interface ICommandHandler<in TCommand, TSuccess>
    where TCommand : ICommand<TSuccess>
{
    Task<CommandResponse<TSuccess>> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}
