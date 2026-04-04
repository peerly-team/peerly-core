using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework.Abstractions;

internal interface IConfirmHomeworkValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, ConfirmHomeworkCommand command, CancellationToken cancellationToken);
}
