
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateHomeworkSubmission;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Proto = Peerly.Core.V1;
namespace Peerly.Core.Api.Controllers.Submissions;

internal static class SubmissionControllerMapper
{
    public static CreateHomeworkSubmissionCommand ToCreateHomeworkSubmissionCommand(this Proto.V1CreateHomeworkSubmissionRequest request)
    {
        return new CreateHomeworkSubmissionCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            StudentId = new StudentId(request.StudentId),
            Comment = request.Comment
        };
    }

    public static Proto.V1CreateHomeworkSubmissionResponse ToV1CreateHomeworkSubmissionResponse(
        this CommandResponse<CreateHomeworkSubmissionCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateHomeworkSubmissionResponse
            {
                SuccessResponse = new Proto.V1CreateHomeworkSubmissionResponse.Types.Success
                {
                    HomeworkSubmissionId = (long)success.HomeworkSubmissionId
                }
            },
            validationError => new Proto.V1CreateHomeworkSubmissionResponse
            {
                ValidationError = validationError.ToProto<CreateHomeworkSubmissionCommand, Proto.V1CreateHomeworkSubmissionRequest>()
            },
            otherError => new Proto.V1CreateHomeworkSubmissionResponse { OtherError = otherError.ToProto() });
    }
}
