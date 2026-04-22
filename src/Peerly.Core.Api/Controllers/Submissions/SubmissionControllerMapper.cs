using System;
using Google.Protobuf.WellKnownTypes;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Tools;
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

    public static UpdateSubmittedHomeworkCommand ToUpdateSubmittedHomeworkCommand(this Proto.V1UpdateSubmittedHomeworkRequest request)
    {
        return new UpdateSubmittedHomeworkCommand
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            StudentId = new StudentId(request.StudentId),
            Comment = request.Comment
        };
    }

    public static Proto.V1UpdateSubmittedHomeworkResponse ToV1UpdateSubmittedHomeworkResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1UpdateSubmittedHomeworkResponse
            {
                SuccessResponse = new Proto.V1UpdateSubmittedHomeworkResponse.Types.Success()
            },
            validationError => new Proto.V1UpdateSubmittedHomeworkResponse
            {
                ValidationError = validationError.ToProto<UpdateSubmittedHomeworkCommand, Proto.V1UpdateSubmittedHomeworkRequest>()
            },
            otherError => new Proto.V1UpdateSubmittedHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static DeleteSubmittedHomeworkCommand ToDeleteSubmittedHomeworkCommand(this Proto.V1DeleteSubmittedHomeworkRequest request)
    {
        return new DeleteSubmittedHomeworkCommand
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1DeleteSubmittedHomeworkResponse ToV1DeleteSubmittedHomeworkResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1DeleteSubmittedHomeworkResponse
            {
                SuccessResponse = new Proto.V1DeleteSubmittedHomeworkResponse.Types.Success()
            },
            validationError => new Proto.V1DeleteSubmittedHomeworkResponse
            {
                ValidationError = validationError.ToProto<DeleteSubmittedHomeworkCommand, Proto.V1DeleteSubmittedHomeworkRequest>()
            },
            otherError => new Proto.V1DeleteSubmittedHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static DeleteSubmittedHomeworkFileCommand ToDeleteSubmittedHomeworkFileCommand(
        this Proto.V1DeleteSubmittedHomeworkFileRequest request)
    {
        return new DeleteSubmittedHomeworkFileCommand
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            FileId = new FileId(request.FileId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1DeleteSubmittedHomeworkFileResponse ToV1DeleteSubmittedHomeworkFileResponse(
        this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1DeleteSubmittedHomeworkFileResponse
            {
                SuccessResponse = new Proto.V1DeleteSubmittedHomeworkFileResponse.Types.Success()
            },
            validationError => new Proto.V1DeleteSubmittedHomeworkFileResponse
            {
                ValidationError = validationError.ToProto<DeleteSubmittedHomeworkFileCommand, Proto.V1DeleteSubmittedHomeworkFileRequest>()
            },
            otherError => new Proto.V1DeleteSubmittedHomeworkFileResponse { OtherError = otherError.ToProto() });
    }

    public static GetSubmittedHomeworkQuery ToGetSubmittedHomeworkQuery(this Proto.V1GetSubmittedHomeworkRequest request)
    {
        return new GetSubmittedHomeworkQuery
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1GetSubmittedHomeworkResponse ToV1GetSubmittedHomeworkResponse(this GetSubmittedHomeworkQueryResponse queryResponse)
    {
        return new Proto.V1GetSubmittedHomeworkResponse
        {
            Submission = new Proto.V1GetSubmittedHomeworkResponse.Types.SubmissionDetail
            {
                SubmittedHomeworkId = (long)queryResponse.Submission.Id,
                Comment = queryResponse.Submission.Comment,
                FinalMark = queryResponse.FinalMark,
                Reviews = { queryResponse.Reviews.ToArrayBy(review => review.ToProto()) },
                Files = { queryResponse.Files.ToArrayBy(file => file.ToProto()) }
            }
        };
    }

    private static Proto.File ToProto(this File file)
    {
        return new Proto.File
        {
            Id = (long)file.Id,
            Name = file.Name,
            Size = file.Size
        };
    }

    private static Proto.V1GetSubmittedHomeworkResponse.Types.Review ToProto(this SubmittedReview review)
    {
        return new Proto.V1GetSubmittedHomeworkResponse.Types.Review
        {
            Mark = review.Mark,
            Comment = review.Comment,
            CreatedAt = Timestamp.FromDateTimeOffset(review.CreationTime)
        };
    }

    public static ListAssignedReviewsQuery ToListAssignedReviewsQuery(this Proto.V1ListAssignedReviewsRequest request)
    {
        return new ListAssignedReviewsQuery
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1ListAssignedReviewsResponse ToV1ListAssignedReviewsResponse(this ListAssignedReviewsQueryResponse queryResponse)
    {
        return new Proto.V1ListAssignedReviewsResponse
        {
            AssignedReviews = { queryResponse.AssignedReviews.ToArrayBy(item => item.ToProto()) }
        };
    }

    private static Proto.V1ListAssignedReviewsResponse.Types.AssignedReview ToProto(this AssignedReview item)
    {
        return new Proto.V1ListAssignedReviewsResponse.Types.AssignedReview
        {
            SubmittedHomeworkId = (long)item.SubmittedHomeworkId,
            HomeworkName = item.HomeworkName,
            IsReviewed = item.IsReviewed
        };
    }

    public static GetAssignedReviewQuery ToGetAssignedReviewQuery(this Proto.V1GetAssignedReviewRequest request)
    {
        return new GetAssignedReviewQuery
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1GetAssignedReviewResponse ToV1GetAssignedReviewResponse(this GetAssignedReviewQueryResponse queryResponse)
    {
        var submission = new Proto.V1GetAssignedReviewResponse.Types.SubmissionForReview
        {
            SubmittedHomeworkId = (long)queryResponse.SubmittedHomeworkId,
            Comment = queryResponse.Comment,
            Checklist = queryResponse.Checklist,
            Files = { queryResponse.Files.ToArrayBy(file => file.ToProto()) }
        };

        if (queryResponse.SubmittedReviewId is { } reviewId)
        {
            submission.SubmittedReviewId = (long)reviewId;
        }

        return new Proto.V1GetAssignedReviewResponse { Submission = submission };
    }

    public static DeleteSubmittedReviewCommand ToDeleteSubmittedReviewCommand(this Proto.V1DeleteSubmittedReviewRequest request)
    {
        return new DeleteSubmittedReviewCommand
        {
            SubmittedReviewId = new SubmittedReviewId(request.SubmittedReviewId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1DeleteSubmittedReviewResponse ToV1DeleteSubmittedReviewResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1DeleteSubmittedReviewResponse
            {
                SuccessResponse = new Proto.V1DeleteSubmittedReviewResponse.Types.Success()
            },
            validationError => new Proto.V1DeleteSubmittedReviewResponse
            {
                ValidationError = validationError.ToProto<DeleteSubmittedReviewCommand, Proto.V1DeleteSubmittedReviewRequest>()
            },
            otherError => new Proto.V1DeleteSubmittedReviewResponse { OtherError = otherError.ToProto() });
    }

    public static UpdateSubmittedReviewCommand ToUpdateSubmittedReviewCommand(this Proto.V1UpdateSubmittedReviewRequest request)
    {
        return new UpdateSubmittedReviewCommand
        {
            SubmittedReviewId = new SubmittedReviewId(request.SubmittedReviewId),
            StudentId = new StudentId(request.StudentId),
            Mark = request.Mark,
            Comment = request.Comment
        };
    }

    public static Proto.V1UpdateSubmittedReviewResponse ToV1UpdateSubmittedReviewResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1UpdateSubmittedReviewResponse
            {
                SuccessResponse = new Proto.V1UpdateSubmittedReviewResponse.Types.Success()
            },
            validationError => new Proto.V1UpdateSubmittedReviewResponse
            {
                ValidationError = validationError.ToProto<UpdateSubmittedReviewCommand, Proto.V1UpdateSubmittedReviewRequest>()
            },
            otherError => new Proto.V1UpdateSubmittedReviewResponse { OtherError = otherError.ToProto() });
    }

    public static CreateSubmittedReviewCommand ToCreateSubmittedReviewCommand(this Proto.V1CreateSubmittedReviewRequest request)
    {
        return new CreateSubmittedReviewCommand
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            StudentId = new StudentId(request.StudentId),
            Mark = request.Mark,
            Comment = request.Comment
        };
    }

    public static Proto.V1CreateSubmittedReviewResponse ToV1CreateSubmittedReviewResponse(
        this CommandResponse<CreateSubmittedReviewCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateSubmittedReviewResponse
            {
                SuccessResponse = new Proto.V1CreateSubmittedReviewResponse.Types.Success
                {
                    SubmittedReviewId = (long)success.SubmittedReviewId
                }
            },
            validationError => new Proto.V1CreateSubmittedReviewResponse
            {
                ValidationError = validationError.ToProto<CreateSubmittedReviewCommand, Proto.V1CreateSubmittedReviewRequest>()
            },
            otherError => new Proto.V1CreateSubmittedReviewResponse { OtherError = otherError.ToProto() });
    }
}
