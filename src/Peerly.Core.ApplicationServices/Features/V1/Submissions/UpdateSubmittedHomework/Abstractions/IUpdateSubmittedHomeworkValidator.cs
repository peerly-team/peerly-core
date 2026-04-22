using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework.Abstractions;

internal interface IUpdateSubmittedHomeworkValidator
{
    Task<OtherError?> RunAsync(UpdateSubmittedHomeworkCommand command, CancellationToken cancellationToken);
}
