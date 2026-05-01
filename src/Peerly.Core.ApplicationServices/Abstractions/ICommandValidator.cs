using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Abstractions;

internal interface ICommandValidator<in TCommand>
{
    Task<CommandValidationResult> ValidateAsync(TCommand command, CancellationToken cancellationToken);
}
