using System;
using Google.Protobuf.WellKnownTypes;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentSubmission;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmissionForReview;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentCourseResults;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmissionDetail;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchHomeworkSubmissions;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchStudentAssignedReviews;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;
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

    public static SearchHomeworkSubmissionsQuery ToSearchHomeworkSubmissionsQuery(this Proto.V1SearchHomeworkSubmissionsRequest request)
    {
        return new SearchHomeworkSubmissionsQuery
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1SearchHomeworkSubmissionsResponse ToV1SearchHomeworkSubmissionsResponse(
        this SearchHomeworkSubmissionsQueryResponse queryResponse)
    {
        return new Proto.V1SearchHomeworkSubmissionsResponse
        {
            Submissions = { queryResponse.Submissions.ToArrayBy(s => s.ToProto()) }
        };
    }

    private static Proto.SubmissionOverviewInfo ToProto(this SubmissionOverviewItem item)
    {
        return new Proto.SubmissionOverviewInfo
        {
            SubmittedHomeworkId = item.SubmittedHomeworkId,
            StudentId = item.StudentId,
            StudentName = item.StudentName,
            ReviewersMark = item.ReviewersMark,
            TeacherMark = item.TeacherMark,
            HasDiscrepancy = item.HasDiscrepancy,
            ReviewsReceived = item.ReviewsReceived
        };
    }

    // V1GetStudentSubmission
    public static GetStudentSubmissionQuery ToGetStudentSubmissionQuery(this Proto.V1GetStudentSubmissionRequest request)
    {
        return new GetStudentSubmissionQuery
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1GetStudentSubmissionResponse ToV1GetStudentSubmissionResponse(
        this GetStudentSubmissionQueryResponse queryResponse)
    {
        var result = new Proto.V1GetStudentSubmissionResponse
        {
            Submission = new Proto.SubmissionDetailInfo
            {
                SubmittedHomeworkId = queryResponse.SubmittedHomeworkId,
                Comment = queryResponse.Comment,
                Files = { queryResponse.Files.ToArrayBy(f => f.ToProtoFileInfo()) },
                ReviewersMark = queryResponse.Mark?.ReviewersMark,
                TeacherMark = queryResponse.Mark?.TeacherMark,
                HasDiscrepancy = queryResponse.Mark?.HasDiscrepancy ?? false
            }
        };
        result.Submission.Reviews.AddRange(queryResponse.Reviews.ToArrayBy(r => new Proto.ReviewInfo
        {
            Mark = r.Mark,
            Comment = r.Comment,
            CreationTime = r.CreationTime.ToTimestamp()
        }));
        return result;
    }

    // V1SearchStudentAssignedReviews
    public static SearchStudentAssignedReviewsQuery ToSearchStudentAssignedReviewsQuery(
        this Proto.V1SearchStudentAssignedReviewsRequest request)
    {
        return new SearchStudentAssignedReviewsQuery
        {
            StudentId = new StudentId(request.StudentId),
            HomeworkId = new HomeworkId(request.HomeworkId)
        };
    }

    public static Proto.V1SearchStudentAssignedReviewsResponse ToV1SearchStudentAssignedReviewsResponse(
        this SearchStudentAssignedReviewsQueryResponse queryResponse)
    {
        return new Proto.V1SearchStudentAssignedReviewsResponse
        {
            AssignedReviews = { queryResponse.AssignedReviews.ToArrayBy(ar => new Proto.AssignedReviewInfo
            {
                SubmittedHomeworkId = (long)ar.SubmittedHomeworkId,
                HomeworkId = (long)ar.HomeworkId,
                HomeworkName = ar.HomeworkName,
                IsReviewed = ar.IsReviewed
            }) }
        };
    }

    // V1GetSubmissionForReview
    public static GetSubmissionForReviewQuery ToGetSubmissionForReviewQuery(
        this Proto.V1GetSubmissionForReviewRequest request)
    {
        return new GetSubmissionForReviewQuery
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1GetSubmissionForReviewResponse ToV1GetSubmissionForReviewResponse(
        this GetSubmissionForReviewQueryResponse queryResponse)
    {
        return new Proto.V1GetSubmissionForReviewResponse
        {
            Submission = new Proto.SubmissionForReviewInfo
            {
                SubmittedHomeworkId = queryResponse.SubmittedHomeworkId,
                Comment = queryResponse.Comment,
                AnonymizedFiles = { queryResponse.AnonymizedFiles.ToArrayBy(f => f.ToProtoFileInfo()) },
                Checklist = queryResponse.Checklist
            }
        };
    }

    // V1GetTeacherSubmissionDetail
    public static GetTeacherSubmissionDetailQuery ToGetTeacherSubmissionDetailQuery(
        this Proto.V1GetTeacherSubmissionDetailRequest request)
    {
        return new GetTeacherSubmissionDetailQuery
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(request.SubmittedHomeworkId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1GetTeacherSubmissionDetailResponse ToV1GetTeacherSubmissionDetailResponse(
        this GetTeacherSubmissionDetailQueryResponse queryResponse)
    {
        var result = new Proto.V1GetTeacherSubmissionDetailResponse
        {
            SubmissionDetail = new Proto.TeacherSubmissionDetailInfo
            {
                SubmittedHomeworkId = queryResponse.SubmittedHomeworkId,
                StudentId = queryResponse.StudentId,
                StudentName = queryResponse.StudentName,
                Comment = queryResponse.Comment,
                Files = { queryResponse.Files.ToArrayBy(f => f.ToProtoFileInfo()) },
                ReviewersMark = queryResponse.Mark?.ReviewersMark,
                TeacherMark = queryResponse.Mark?.TeacherMark,
                HasDiscrepancy = queryResponse.Mark?.HasDiscrepancy ?? false
            }
        };
        result.SubmissionDetail.Reviews.AddRange(queryResponse.Reviews.ToArrayBy(rws => new Proto.TeacherReviewInfo
        {
            ReviewerStudentId = (long)rws.Review.StudentId,
            ReviewerName = rws.Reviewer?.Name ?? rws.Reviewer?.Email ?? string.Empty,
            Mark = rws.Review.Mark,
            Comment = rws.Review.Comment,
            CreationTime = rws.Review.CreationTime.ToTimestamp()
        }));
        return result;
    }

    // V1GetStudentCourseResults
    public static GetStudentCourseResultsQuery ToGetStudentCourseResultsQuery(
        this Proto.V1GetStudentCourseResultsRequest request)
    {
        return new GetStudentCourseResultsQuery
        {
            StudentId = new StudentId(request.StudentId),
            CourseId = new CourseId(request.CourseId)
        };
    }

    public static Proto.V1GetStudentCourseResultsResponse ToV1GetStudentCourseResultsResponse(
        this GetStudentCourseResultsQueryResponse queryResponse)
    {
        return new Proto.V1GetStudentCourseResultsResponse
        {
            Results = { queryResponse.Results.ToArrayBy(r => new Proto.StudentHomeworkResult
            {
                HomeworkId = r.HomeworkId,
                HomeworkName = r.HomeworkName,
                HomeworkStatus = r.HomeworkStatus.ToProto(),
                ReviewersMark = r.ReviewersMark,
                TeacherMark = r.TeacherMark
            }) }
        };
    }

    private static Proto.HomeworkStatus ToProto(this HomeworkStatus homeworkStatus)
    {
        return homeworkStatus switch
        {
            HomeworkStatus.Draft => Proto.HomeworkStatus.Draft,
            HomeworkStatus.Published => Proto.HomeworkStatus.Published,
            HomeworkStatus.Reviewing => Proto.HomeworkStatus.Reviewing,
            HomeworkStatus.Confirmation => Proto.HomeworkStatus.Confirmation,
            HomeworkStatus.Finished => Proto.HomeworkStatus.Finished,
            _ => throw new ArgumentOutOfRangeException(nameof(homeworkStatus), homeworkStatus, null)
        };
    }

    private static Proto.FileInfo ToProtoFileInfo(this File file)
    {
        return new Proto.FileInfo
        {
            Id = (long)file.Id,
            Name = file.Name,
            Size = file.Size
        };
    }
}
