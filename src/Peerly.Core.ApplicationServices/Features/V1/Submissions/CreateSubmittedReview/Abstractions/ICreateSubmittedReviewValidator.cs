using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview.Abstractions;

internal interface ICreateSubmittedReviewValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, CreateSubmittedReviewCommand command, CancellationToken cancellationToken);
}
