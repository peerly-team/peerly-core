using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines.Abstractions;

internal interface IPostponeHomeworkDeadlinesValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, PostponeHomeworkDeadlinesCommand command, CancellationToken cancellationToken);
}
