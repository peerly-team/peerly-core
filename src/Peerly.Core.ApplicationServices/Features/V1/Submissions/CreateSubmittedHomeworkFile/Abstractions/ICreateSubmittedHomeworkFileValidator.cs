using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.Abstractions;

internal interface ICreateSubmittedHomeworkFileValidator
{
    Task<OtherError?> RunAsync(CreateSubmittedHomeworkFileCommand command, CancellationToken cancellationToken);
}
