using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework.Abstractions;

internal interface ICreateGroupHomeworkValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, CreateGroupHomeworkCommand command, CourseId courseId, CancellationToken cancellationToken);
}
