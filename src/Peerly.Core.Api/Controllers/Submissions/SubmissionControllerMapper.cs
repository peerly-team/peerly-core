using System;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Submissions;

internal static class SubmissionControllerMapper
{
    public static CreateSubmittedHomeworkCommand ToCreateSubmittedHomeworkCommand(this Proto.V1CreateSubmittedHomeworkRequest request)
    {
        return new CreateSubmittedHomeworkCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            StudentId = new StudentId(request.StudentId),
            Comment = request.Comment
        };
    }

    public static Proto.V1CreateSubmittedHomeworkResponse ToV1CreateSubmittedHomeworkResponse(
        this CommandResponse<CreateSubmittedHomeworkCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateSubmittedHomeworkResponse
            {
                SuccessResponse = new Proto.V1CreateSubmittedHomeworkResponse.Types.Success
                {
                    SubmittedHomeworkId = (long)success.SubmittedHomeworkId
                }
            },
            validationError => new Proto.V1CreateSubmittedHomeworkResponse
            {
                ValidationError = validationError.ToProto<CreateSubmittedHomeworkCommand, Proto.V1CreateSubmittedHomeworkRequest>()
            },
            otherError => new Proto.V1CreateSubmittedHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static CreateSubmittedHomeworkFileCommand ToCreateSubmittedHomeworkFileCommand(
        this Proto.V1CreateSubmittedHomeworkFileRequest request)
    {
        return new CreateSubmittedHomeworkFileCommand
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            StorageId = (StorageId)Guid.Parse(request.StorageId),
            FileName = request.FileName,
            FileSize = request.FileSize,
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1CreateSubmittedHomeworkFileResponse ToV1CreateSubmittedHomeworkFileResponse(
        this CommandResponse<CreateSubmittedHomeworkFileCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateSubmittedHomeworkFileResponse
            {
                SuccessResponse = new Proto.V1CreateSubmittedHomeworkFileResponse.Types.Success
                {
                    FileId = (long)success.FileId
                }
            },
            validationError => new Proto.V1CreateSubmittedHomeworkFileResponse
            {
                ValidationError = validationError.ToProto<CreateSubmittedHomeworkFileCommand, Proto.V1CreateSubmittedHomeworkFileRequest>()
            },
            otherError => new Proto.V1CreateSubmittedHomeworkFileResponse { OtherError = otherError.ToProto() });
    }
}
