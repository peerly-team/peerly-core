using System;
using Google.Protobuf.WellKnownTypes;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomework;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.SearchCourseHomeworks;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.UpdateHomeworkStatus;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Pagination;
using Peerly.Core.Tools;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Homeworks;

internal static class HomeworkControllerMapper
{
    public static CreateHomeworkCommand ToCreateHomeworkCommand(this Proto.V1CreateHomeworkRequest request)
    {
        return new CreateHomeworkCommand
        {
            CourseId = new CourseId(request.CourseId),
            GroupId = request.GroupId is not null ? new GroupId(request.GroupId.Value) : null,
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name,
            AmountOfReviewers = request.AmountOfReviewers,
            Description = request.Description,
            Checklist = request.Checklist,
            Deadline = request.Deadline.ToDateTimeOffset(),
            ReviewDeadline = request.ReviewDeadline.ToDateTimeOffset()
        };
    }

    public static Proto.V1CreateHomeworkResponse ToV1CreateHomeworkResponse(
        this CommandResponse<CreateHomeworkCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateHomeworkResponse
            {
                SuccessResponse = new Proto.V1CreateHomeworkResponse.Types.Success
                {
                    HomeworkId = (long)success.HomeworkId
                }
            },
            validationError => new Proto.V1CreateHomeworkResponse
            {
                ValidationError = validationError.ToProto<CreateHomeworkCommand, Proto.V1CreateHomeworkRequest>()
            },
            otherError => new Proto.V1CreateHomeworkResponse { OtherError = otherError.ToProto() });
    }

    public static UpdateHomeworkStatusCommand ToUpdateHomeworkStatusCommand(this Proto.V1UpdateHomeworkStatusRequest request)
    {
        return new UpdateHomeworkStatusCommand
        {
            HomeworkId = new HomeworkId(request.HomeworkId),
            TeacherId = new TeacherId(request.TeacherId),
            HomeworkStatus = request.HomeworkStatus.ToModel(),
        };
    }

    public static Proto.V1UpdateHomeworkStatusResponse ToV1UpdateHomeworkStatusResponse(
        this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1UpdateHomeworkStatusResponse { SuccessResponse = new Proto.V1UpdateHomeworkStatusResponse.Types.Success() },
            validationError => new Proto.V1UpdateHomeworkStatusResponse
            {
                ValidationError = validationError.ToProto<UpdateHomeworkStatusCommand, Proto.V1UpdateHomeworkStatusRequest>()
            },
            otherError => new Proto.V1UpdateHomeworkStatusResponse { OtherError = otherError.ToProto() });
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
}
