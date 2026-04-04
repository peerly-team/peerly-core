using System.Collections.Generic;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework.Abstractions;

internal interface IConfirmHomeworkHandlerMapper
{
    IReadOnlyCollection<SubmittedHomeworkMarkBatchUpdateItem> ToMarkBatchUpdateItems(ConfirmHomeworkCommand command);
}
