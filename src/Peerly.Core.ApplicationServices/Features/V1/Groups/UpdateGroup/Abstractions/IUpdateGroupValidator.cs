using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup.Abstractions;

internal interface IUpdateGroupValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, UpdateGroupCommand command, CancellationToken cancellationToken);
}
