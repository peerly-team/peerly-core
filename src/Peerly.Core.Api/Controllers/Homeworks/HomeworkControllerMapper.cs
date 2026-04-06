using System;
using Google.Protobuf.WellKnownTypes;
using OneOf.Types;
using System.Linq;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.ConfirmHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PostponeHomeworkDeadlines;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.PublishHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.GetHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateDraftHomework;
using Peerly.Core.Models.Files;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.SearchCourseHomeworks;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Pagination;
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
            TeacherId = new TeacherId(request.TeacherId),
            MarkCorrections = request.MarkCorrections
                .Select(mc => new MarkCorrection
                {
                    SubmittedHomeworkId = new SubmittedHomeworkId(mc.SubmittedHomeworkId),
                    TeacherMark = mc.TeacherMark
                })
                .ToArray()
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

    public static GetTeacherHomeworkQuery ToGetTeacherHomeworkQuery(this Proto.V1GetTeacherHomeworkRequest request)
    {
        return new GetTeacherHomeworkQuery
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1GetTeacherHomeworkResponse ToV1GetTeacherHomeworkResponse(this GetTeacherHomeworkQueryResponse queryResponse)
    {
        return new Proto.V1GetTeacherHomeworkResponse
        {
            HomeworkDetail = queryResponse.HomeworkDetail.ToProtoDetail()
        };
    }

    public static GetStudentHomeworkQuery ToGetStudentHomeworkQuery(this Proto.V1GetStudentHomeworkRequest request)
    {
        return new GetStudentHomeworkQuery
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1GetStudentHomeworkResponse ToV1GetStudentHomeworkResponse(this GetStudentHomeworkQueryResponse queryResponse)
    {
        return new Proto.V1GetStudentHomeworkResponse
        {
            HomeworkDetail = queryResponse.HomeworkDetail.ToProtoDetail()
        };
    }

    public static SearchTeacherCourseHomeworksQuery ToSearchTeacherCourseHomeworksQuery(this Proto.V1SearchTeacherCoursesHomeworksRequest request)
    {
        return new SearchTeacherCourseHomeworksQuery
        {
            TeacherId = new TeacherId(request.TeacherId),
            CourseId = new CourseId(request.CourseId),
            Filter = request.Filter.ToFilter(),
            PaginationInfo = request.PaginationInfo.ToPaginationInfo()
        };
    }

    public static Proto.V1SearchTeacherCoursesHomeworksResponse ToV1SearchTeacherCoursesHomeworksResponse(
        this SearchTeacherCourseHomeworksQueryResponse queryResponse)
    {
        return new Proto.V1SearchTeacherCoursesHomeworksResponse
        {
            HomeworkInfos = { queryResponse.Homeworks.ToArrayBy(homework => homework.ToProto()) }
        };
    }

    public static SearchStudentCourseHomeworksQuery ToSearchStudentCoursesQuery(this Proto.V1SearchStudentCourseHomeworksRequest request)
    {
        return new SearchStudentCourseHomeworksQuery
        {
            StudentId = new StudentId(request.StudentId),
            CourseId = new CourseId(request.CourseId),
            Filter = request.Filter.ToFilter(),
            PaginationInfo = request.PaginationInfo.ToPaginationInfo()
        };
    }

    public static Proto.V1SearchStudentCourseHomeworksResponse ToV1SearchStudentCoursesResponse(
        this SearchStudentCourseHomeworksQueryResponse queryResponse)
    {
        return new Proto.V1SearchStudentCourseHomeworksResponse
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
            AmountOfReviewers = homework.AmountOfReviewers
        };
    }

    private static SearchCourseHomeworksQueryFilter ToFilter(this Proto.SearchCourseHomeworksFilter filterProto)
    {
        return new SearchCourseHomeworksQueryFilter
        {
            HomeworkStatuses = filterProto.HomeworkStatuses.ToArrayBy(homeworkStatus => homeworkStatus.ToModel())
        };
    }

    private static HomeworkStatus ToModel(this Proto.HomeworkStatus homeworkStatusProto)
    {
        return homeworkStatusProto switch
        {
            Proto.HomeworkStatus.Draft => HomeworkStatus.Draft,
            Proto.HomeworkStatus.Published => HomeworkStatus.Published,
            Proto.HomeworkStatus.Reviewing => HomeworkStatus.Reviewing,
            Proto.HomeworkStatus.Confirmation => HomeworkStatus.Confirmation,
            Proto.HomeworkStatus.Finished => HomeworkStatus.Finished,
            _ => throw new ArgumentOutOfRangeException(nameof(homeworkStatusProto), homeworkStatusProto, null)
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

    private static PaginationInfo ToPaginationInfo(this Proto.PaginationInfo paginationInfoProto)
    {
        return new PaginationInfo
        {
            Offset = paginationInfoProto.Offset,
            PageSize = paginationInfoProto.PageSize
        };
    }

    private static Proto.HomeworkDetailInfo ToProtoDetail(this HomeworkDetailResponseItem item)
    {
        var homework = item.Homework;
        var result = new Proto.HomeworkDetailInfo
        {
            Id = (long)homework.Id,
            Name = homework.Name,
            Status = homework.Status.ToProto(),
            Description = homework.Description,
            Checklist = homework.CheckList,
            Deadline = homework.Deadline.ToTimestamp(),
            ReviewDeadline = homework.ReviewDeadline.ToTimestamp(),
            AmountOfReviewers = homework.AmountOfReviewers,
            DiscrepancyThreshold = homework.DiscrepancyThreshold,
            GroupId = homework.GroupId.HasValue ? (long)homework.GroupId.Value : null,
            CourseId = (long)homework.CourseId,
            Files = { item.Files.ToArrayBy(f => f.ToProtoFileInfo()) }
        };

        return result;
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
