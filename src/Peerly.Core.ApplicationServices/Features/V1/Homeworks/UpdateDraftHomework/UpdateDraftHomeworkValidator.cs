using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;

internal sealed class UpdateDraftHomeworkValidator : IUpdateDraftHomeworkValidator
{
    public async Task<OtherError?> ValidateAsync(
        ICommonUnitOfWork unitOfWork,
        UpdateDraftHomeworkCommand command,
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

        if (homework.Status is not HomeworkStatus.Draft)
        {
            return OtherError.Conflict();
        }

        return null;
    }
}
