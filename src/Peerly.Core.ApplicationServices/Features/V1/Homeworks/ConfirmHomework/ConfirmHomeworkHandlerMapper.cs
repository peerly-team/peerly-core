using System.Collections.Generic;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework.Abstractions;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;

internal sealed class ConfirmHomeworkHandlerMapper : IConfirmHomeworkHandlerMapper
{
    private readonly IClock _clock;

    public ConfirmHomeworkHandlerMapper(IClock clock)
    {
        _clock = clock;
    }

    public IReadOnlyCollection<SubmittedHomeworkMarkBatchUpdateItem> ToMarkBatchUpdateItems(ConfirmHomeworkCommand command)
    {
        var currentTime = _clock.GetCurrentTime();
        return command.MarkCorrections
            .ToArrayBy(correction => new SubmittedHomeworkMarkBatchUpdateItem
            {
                SubmittedHomeworkId = correction.SubmittedHomeworkId,
                TeacherMark = correction.TeacherMark,
                TeacherId = command.TeacherId,
                UpdateTime = currentTime
            });
    }
}
