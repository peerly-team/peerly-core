using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher.Abstractions;

internal interface IAddGroupTeacherValidator
{
    Task<OtherError?> ValidateAsync(ICommonUnitOfWork unitOfWork, AddGroupTeacherCommand command, CancellationToken cancellationToken);
}
