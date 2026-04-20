using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Validators.HomeworkSubmissionAccess.Abstractions;

internal interface IHomeworkSubmissionAccessValidator
{
    Task<OtherError?> RunAsync(HomeworkId homeworkId, StudentId studentId, CancellationToken cancellationToken);
}
