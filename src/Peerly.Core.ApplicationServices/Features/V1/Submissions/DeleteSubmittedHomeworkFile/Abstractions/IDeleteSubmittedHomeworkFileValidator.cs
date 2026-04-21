using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile.Abstractions;

internal interface IDeleteSubmittedHomeworkFileValidator
{
    Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        DeleteSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken);
}
