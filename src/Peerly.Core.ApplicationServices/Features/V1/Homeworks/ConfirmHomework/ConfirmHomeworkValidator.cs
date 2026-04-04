using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;

internal sealed class ConfirmHomeworkValidator : IConfirmHomeworkValidator
{
    public async Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        ConfirmHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await unitOfWork.HomeworkRepository.GetAsync(command.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound();
        }

        if (homework.TeacherId != command.TeacherId)
        {
            return OtherError.PermissionDenied();
        }

        if (homework.Status is not HomeworkStatus.Confirmation)
        {
            return OtherError.Conflict();
        }

        return null;
    }
}
