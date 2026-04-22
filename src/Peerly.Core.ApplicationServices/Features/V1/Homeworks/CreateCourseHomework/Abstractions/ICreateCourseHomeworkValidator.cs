using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework.Abstractions;

internal interface ICreateCourseHomeworkValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, CreateCourseHomeworkCommand command, CancellationToken cancellationToken);
}
