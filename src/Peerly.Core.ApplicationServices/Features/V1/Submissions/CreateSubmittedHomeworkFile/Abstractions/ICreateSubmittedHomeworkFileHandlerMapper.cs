using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Models.Files;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.Abstractions;

internal interface ICreateSubmittedHomeworkFileHandlerMapper
{
    FileAddItem ToFileAddItem(CreateSubmittedHomeworkFileCommand command);
    FileAddItem ToAnonymizedFileAddItem(CreateSubmittedHomeworkFileCommand command, AnonymizationResponse response);
}
