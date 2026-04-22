using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework.Abstractions;

internal interface IDeleteSubmittedHomeworkValidator
{
    Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        DeleteSubmittedHomeworkCommand command,
        CancellationToken cancellationToken);
}
