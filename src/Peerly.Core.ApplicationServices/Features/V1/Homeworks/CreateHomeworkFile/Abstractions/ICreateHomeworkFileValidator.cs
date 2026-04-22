using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile.Abstractions;

internal interface ICreateHomeworkFileValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, CreateHomeworkFileCommand command, CancellationToken cancellationToken);
}
