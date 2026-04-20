using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework.Abstractions;

internal interface ICreateSubmittedHomeworkValidator
{
    Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        CreateSubmittedHomeworkCommand command,
        CancellationToken cancellationToken);
}
