using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup.Abstractions;

internal interface ICreateGroupValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, CreateGroupCommand command, CancellationToken cancellationToken);
}
