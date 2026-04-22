using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup.Abstractions;

internal interface IDeleteGroupValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, DeleteGroupCommand command, CancellationToken cancellationToken);
}
