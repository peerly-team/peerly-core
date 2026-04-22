using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework.Abstractions;

internal interface IUpdateDraftHomeworkValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, UpdateDraftHomeworkCommand command, CancellationToken cancellationToken);
}
