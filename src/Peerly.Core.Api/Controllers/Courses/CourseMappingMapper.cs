using System;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.PublishCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Pagination;
using Peerly.Core.Tools;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Courses;

internal static class CourseMappingMapper
{
    public static GetTeacherCourseQuery ToGetTeacherCourseQuery(this Proto.V1GetTeacherCourseRequest request)
    {
        return new GetTeacherCourseQuery
        {
            CourseId = new CourseId(request.CourseId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1GetTeacherCourseResponse ToV1GetTeacherCourseResponse(this GetTeacherCourseQueryResponse queryResponse)
    {
        return new Proto.V1GetTeacherCourseResponse
        {
            CourseInfo = queryResponse.Course.ToProto(),
            StudentCount = queryResponse.StudentCount,
            HomeworkCount = queryResponse.HomeworkCount
        };
    }

    public static GetStudentCourseQuery ToGetStudentCourseQuery(this Proto.V1GetStudentCourseRequest request)
    {
        return new GetStudentCourseQuery
        {
            CourseId = new CourseId(request.CourseId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1GetStudentCourseResponse ToV1GetStudentCourseResponse(this GetStudentCourseQueryResponse queryResponse)
    {
        return new Proto.V1GetStudentCourseResponse
        {
            CourseInfo = queryResponse.Course.ToProto(),
            StudentCount = queryResponse.StudentCount,
            HomeworkCount = queryResponse.HomeworkCount
        };
    }

    public static CreateCourseCommand ToCreateCourseCommand(this Proto.V1CreateCourseRequest request)
    {
        return new CreateCourseCommand
        {
            Name = request.Name,
            Description = request.Description,
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1CreateCourseResponse ToV1CreateCourseResponse(this CommandResponse<CreateCourseCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateCourseResponse
            {
                SuccessResponse = new Proto.V1CreateCourseResponse.Types.Success
                {
                    CourseId = (long)success.CourseId
                }
            },
            validationError => new Proto.V1CreateCourseResponse
            {
                ValidationError = validationError.ToProto<CreateCourseCommand, Proto.V1CreateCourseRequest>()
            },
            otherError => new Proto.V1CreateCourseResponse { OtherError = otherError.ToProto() });
    }

    public static DeleteCourseCommand ToDeleteCourseCommand(this Proto.V1DeleteCourseRequest request)
    {
        return new DeleteCourseCommand
        {
            CourseId = new CourseId(request.CourseId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1DeleteCourseResponse ToV1DeleteCourseResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1DeleteCourseResponse { SuccessResponse = new Proto.V1DeleteCourseResponse.Types.Success() },
            validationError => new Proto.V1DeleteCourseResponse
            {
                ValidationError = validationError.ToProto<DeleteCourseCommand, Proto.V1DeleteCourseRequest>()
            },
            otherError => new Proto.V1DeleteCourseResponse { OtherError = otherError.ToProto() });
    }

    public static UpdateCourseCommand ToUpdateCourseCommand(this Proto.V1UpdateCourseRequest request)
    {
        return new UpdateCourseCommand
        {
            CourseId = new CourseId(request.CourseId),
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name,
            Description = request.Description
        };
    }

    public static Proto.V1UpdateCourseResponse ToV1UpdateCourseResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1UpdateCourseResponse { SuccessResponse = new Proto.V1UpdateCourseResponse.Types.Success() },
            validationError => new Proto.V1UpdateCourseResponse
            {
                ValidationError = validationError.ToProto<UpdateCourseCommand, Proto.V1UpdateCourseRequest>()
            },
            otherError => new Proto.V1UpdateCourseResponse { OtherError = otherError.ToProto() });
    }

    public static PublishCourseCommand ToPublishCourseCommand(this Proto.V1PublishCourseRequest request)
    {
        return new PublishCourseCommand
        {
            CourseId = new CourseId(request.CourseId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1PublishCourseResponse ToV1PublishCourseResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1PublishCourseResponse { SuccessResponse = new Proto.V1PublishCourseResponse.Types.Success() },
            validationError => new Proto.V1PublishCourseResponse
            {
                ValidationError = validationError.ToProto<PublishCourseCommand, Proto.V1PublishCourseRequest>()
            },
            otherError => new Proto.V1PublishCourseResponse { OtherError = otherError.ToProto() });
    }

    public static SearchStudentCoursesQuery ToSearchStudentCoursesQuery(this Proto.V1SearchStudentCoursesRequest request)
    {
        return new SearchStudentCoursesQuery
        {
            StudentId = new StudentId(request.StudentId),
            Filter = request.Filter.ToFilter(),
            PaginationInfo = request.PaginationInfo.ToPaginationInfo()
        };
    }

    public static Proto.V1SearchStudentCoursesResponse ToV1SearchStudentCoursesResponse(
        this SearchStudentCoursesQueryResponse queryResponse)
    {
        return new Proto.V1SearchStudentCoursesResponse
        {
            CourseInfos = { queryResponse.Courses.ToArrayBy(course => course.ToProto()) }
        };
    }

    public static SearchTeacherCoursesQuery ToSearchTeacherCoursesQuery(this Proto.V1SearchTeacherCoursesRequest request)
    {
        return new SearchTeacherCoursesQuery
        {
            TeacherId = new TeacherId(request.TeacherId),
            Filter = request.Filter.ToFilter(),
            PaginationInfo = request.PaginationInfo.ToPaginationInfo()
        };
    }

    public static Proto.V1SearchTeacherCoursesResponse ToV1SearchTeacherCoursesResponse(
        this SearchTeacherCoursesQueryResponse queryResponse)
    {
        return new Proto.V1SearchTeacherCoursesResponse
        {
            CourseInfos = { queryResponse.Courses.ToArrayBy(courseInfo => courseInfo.ToProto()) }
        };
    }

    private static Proto.CourseInfo ToProto(this Course course)
    {
        return new Proto.CourseInfo
        {
            Id = (long)course.Id,
            Name = course.Name,
            Description = course.Description,
            Status = course.Status.ToProto()
        };
    }

    private static SearchCoursesQueryFilter ToFilter(this Proto.SearchCoursesFilter filterProto)
    {
        return new SearchCoursesQueryFilter
        {
            CourseStatuses = filterProto.CourseStatuses.ToArrayBy(courseStatus => courseStatus.ToModel())
        };
    }

    private static CourseStatus ToModel(this Proto.CourseStatus courseStatusProto)
    {
        return courseStatusProto switch
        {
            Proto.CourseStatus.Draft => CourseStatus.Draft,
            Proto.CourseStatus.InProgress => CourseStatus.InProgress,
            Proto.CourseStatus.Finished => CourseStatus.Finished,
            Proto.CourseStatus.Canceled => CourseStatus.Canceled,
            Proto.CourseStatus.Deleted => CourseStatus.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(courseStatusProto), courseStatusProto, null)
        };
    }

    private static Proto.CourseStatus ToProto(this CourseStatus courseStatus)
    {
        return courseStatus switch
        {
            CourseStatus.Draft => Proto.CourseStatus.Draft,
            CourseStatus.InProgress => Proto.CourseStatus.InProgress,
            CourseStatus.Finished => Proto.CourseStatus.Finished,
            CourseStatus.Canceled => Proto.CourseStatus.Canceled,
            CourseStatus.Deleted => Proto.CourseStatus.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(courseStatus), courseStatus, null)
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
