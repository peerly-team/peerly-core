using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent.Abstractions;

internal interface IAddGroupStudentValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, AddGroupStudentCommand command, CancellationToken cancellationToken);
}
