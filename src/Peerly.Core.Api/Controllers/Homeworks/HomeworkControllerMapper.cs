using System;
using Google.Protobuf.WellKnownTypes;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Tools;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Homeworks;

internal static class HomeworkControllerMapper
{
    public static CreateCourseHomeworkCommand ToCreateCourseHomeworkCommand(this Proto.V1CreateCourseHomeworkRequest request)
    {
        return new CreateCourseHomeworkCommand
        {
            CourseId = new CourseId(request.CourseId),
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name,
            AmountOfReviewers = request.AmountOfReviewers,
            Description = request.Description,
            Checklist = request.Checklist,
            Deadline = request.Deadline.ToDateTimeOffset(),
            ReviewDeadline = request.ReviewDeadline.ToDateTimeOffset(),
            DiscrepancyThreshold = request.DiscrepancyThreshold
        };
    }

    public static Proto.V1CreateCourseHomeworkResponse ToV1CreateCourseHomeworkResponse(
        this CommandResponse<CreateCourseHomeworkCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateCourseHomeworkResponse
            {
                SuccessResponse = new Proto.V1CreateCourseHomeworkResponse.Types.Success
                {
                    HomeworkId = (long)success.HomeworkId
                }
            },
            validationError => new Proto.V1CreateCourseHomeworkResponse
            {
                ValidationError = validationError.ToProto<CreateCourseHomeworkCommand, Proto.V1CreateCourseHomeworkRequest>()
            },
            otherError => new Proto.V1CreateCourseHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static CreateGroupHomeworkCommand ToCreateGroupHomeworkCommand(this Proto.V1CreateGroupHomeworkRequest request)
    {
        return new CreateGroupHomeworkCommand
        {
            GroupId = new GroupId(request.GroupId),
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name,
            AmountOfReviewers = request.AmountOfReviewers,
            Description = request.Description,
            Checklist = request.Checklist,
            Deadline = request.Deadline.ToDateTimeOffset(),
            ReviewDeadline = request.ReviewDeadline.ToDateTimeOffset(),
            DiscrepancyThreshold = request.DiscrepancyThreshold
        };
    }

    public static Proto.V1CreateGroupHomeworkResponse ToV1CreateGroupHomeworkResponse(
        this CommandResponse<CreateGroupHomeworkCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateGroupHomeworkResponse
            {
                SuccessResponse = new Proto.V1CreateGroupHomeworkResponse.Types.Success
                {
                    HomeworkId = (long)success.HomeworkId
                }
            },
            validationError => new Proto.V1CreateGroupHomeworkResponse
            {
                ValidationError = validationError.ToProto<CreateGroupHomeworkCommand, Proto.V1CreateGroupHomeworkRequest>()
            },
            otherError => new Proto.V1CreateGroupHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static PublishHomeworkCommand ToPublishHomeworkCommand(this Proto.V1PublishHomeworkRequest request)
    {
        return new PublishHomeworkCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1PublishHomeworkResponse ToV1PublishHomeworkResponse(
        this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1PublishHomeworkResponse { SuccessResponse = new Proto.V1PublishHomeworkResponse.Types.Success() },
            validationError => new Proto.V1PublishHomeworkResponse
            {
                ValidationError = validationError.ToProto<PublishHomeworkCommand, Proto.V1PublishHomeworkRequest>()
            },
            otherError => new Proto.V1PublishHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static ConfirmHomeworkCommand ToConfirmHomeworkCommand(this Proto.V1ConfirmHomeworkRequest request)
    {
        return new ConfirmHomeworkCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1ConfirmHomeworkResponse ToV1ConfirmHomeworkResponse(
        this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1ConfirmHomeworkResponse { SuccessResponse = new Proto.V1ConfirmHomeworkResponse.Types.Success() },
            validationError => new Proto.V1ConfirmHomeworkResponse
            {
                ValidationError = validationError.ToProto<ConfirmHomeworkCommand, Proto.V1ConfirmHomeworkRequest>()
            },
            otherError => new Proto.V1ConfirmHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static CreateHomeworkFileCommand ToCreateHomeworkFileCommand(this Proto.V1CreateHomeworkFileRequest request)
    {
        return new CreateHomeworkFileCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            StorageId = (StorageId)Guid.Parse(request.StorageId),
            FileName = request.FileName,
            FileSize = request.FileSize,
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1CreateHomeworkFileResponse ToV1CreateHomeworkAttachmentResponse(
        this CommandResponse<CreateHomeworkFileCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateHomeworkFileResponse
            {
                SuccessResponse = new Proto.V1CreateHomeworkFileResponse.Types.Success
                {
                    FileId = (long)success.FileId
                }
            },
            validationError => new Proto.V1CreateHomeworkFileResponse
            {
                ValidationError = validationError.ToProto<CreateHomeworkFileCommand, Proto.V1CreateHomeworkFileRequest>()
            },
            otherError => new Proto.V1CreateHomeworkFileResponse { OtherError = otherError.ToProto() });
    }

    public static UpdateDraftHomeworkCommand ToUpdateDraftHomeworkCommand(this Proto.V1UpdateDraftHomeworkRequest request)
    {
        return new UpdateDraftHomeworkCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name,
            AmountOfReviewers = request.AmountOfReviewers,
            Description = request.Description,
            Checklist = request.Checklist,
            Deadline = request.Deadline.ToDateTimeOffset(),
            ReviewDeadline = request.ReviewDeadline.ToDateTimeOffset(),
            DiscrepancyThreshold = request.DiscrepancyThreshold
        };
    }

    public static Proto.V1UpdateDraftHomeworkResponse ToV1UpdateDraftHomeworkResponse(
        this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1UpdateDraftHomeworkResponse { SuccessResponse = new Proto.V1UpdateDraftHomeworkResponse.Types.Success() },
            validationError => new Proto.V1UpdateDraftHomeworkResponse
            {
                ValidationError = validationError.ToProto<UpdateDraftHomeworkCommand, Proto.V1UpdateDraftHomeworkRequest>()
            },
            otherError => new Proto.V1UpdateDraftHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static PostponeHomeworkDeadlinesCommand ToPostponeHomeworkDeadlinesCommand(this Proto.V1PostponeHomeworkDeadlinesRequest request)
    {
        return new PostponeHomeworkDeadlinesCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            TeacherId = new TeacherId(request.TeacherId),
            Deadline = request.Deadline?.ToDateTimeOffset(),
            ReviewDeadline = request.ReviewDeadline?.ToDateTimeOffset()
        };
    }

    public static Proto.V1PostponeHomeworkDeadlinesResponse ToV1PostponeHomeworkDeadlinesResponse(
        this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1PostponeHomeworkDeadlinesResponse { SuccessResponse = new Proto.V1PostponeHomeworkDeadlinesResponse.Types.Success() },
            validationError => new Proto.V1PostponeHomeworkDeadlinesResponse
            {
                ValidationError = validationError.ToProto<PostponeHomeworkDeadlinesCommand, Proto.V1PostponeHomeworkDeadlinesRequest>()
            },
            otherError => new Proto.V1PostponeHomeworkDeadlinesResponse { OtherError = otherError.ToProto() });
    }

    public static ListStudentCourseHomeworksQuery ToListStudentCourseHomeworksQuery(this Proto.V1ListStudentCourseHomeworksRequest request)
    {
        return new ListStudentCourseHomeworksQuery
        {
            StudentId = new StudentId(request.StudentId),
            CourseId = new CourseId(request.CourseId)
        };
    }

    public static Proto.V1ListStudentCourseHomeworksResponse ToV1ListStudentCourseHomeworksResponse(
        this ListStudentCourseHomeworksQueryResponse queryResponse)
    {
        return new Proto.V1ListStudentCourseHomeworksResponse
        {
            HomeworkInfos = { queryResponse.Homeworks.ToArrayBy(homework => homework.ToProto()) }
        };
    }

    public static ListTeacherCourseHomeworksQuery ToListTeacherCourseHomeworksQuery(this Proto.V1ListTeacherCourseHomeworksRequest request)
    {
        return new ListTeacherCourseHomeworksQuery
        {
            TeacherId = new TeacherId(request.TeacherId),
            CourseId = new CourseId(request.CourseId)
        };
    }

    public static Proto.V1ListTeacherCourseHomeworksResponse ToV1ListTeacherCourseHomeworksResponse(
        this ListTeacherCourseHomeworksQueryResponse queryResponse)
    {
        return new Proto.V1ListTeacherCourseHomeworksResponse
        {
            HomeworkInfos = { queryResponse.Homeworks.ToArrayBy(homework => homework.ToProto()) }
        };
    }

    private static Proto.HomeworkInfo ToProto(this Homework homework)
    {
        return new Proto.HomeworkInfo
        {
            Id = (long)homework.Id,
            Name = homework.Name,
            Status = homework.Status.ToProto(),
            Description = homework.Description,
            Checklist = homework.CheckList,
            Deadline = homework.Deadline.ToTimestamp(),
            ReviewDeadline = homework.ReviewDeadline.ToTimestamp(),
            AmountOfReviewers = homework.AmountOfReviewers,
            DiscrepancyThreshold = homework.DiscrepancyThreshold
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
}
