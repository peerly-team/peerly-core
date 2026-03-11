using System;
using System.Diagnostics.CodeAnalysis;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;
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

[ExcludeFromCodeCoverage]
internal static class CourseMappingExtensions
{
    public static CreateCourseCommand ToCreateCourseCommand(this Proto.V1CreateCourseRequest request)
    {
        return new CreateCourseCommand
        {
            Name = request.Name,
            Description = request.Description,
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1CreateCourseResponse ToV1CreateCourseResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1CreateCourseResponse { SuccessResponse = new Proto.V1CreateCourseResponse.Types.Success() },
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
            Description = request.Description,
            Status = request.Status.ToModel()
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

    public static SearchCoursesQuery ToSearchCoursesQuery(this Proto.V1SearchCoursesRequest request)
    {
        return new SearchCoursesQuery
        {
            Filter = request.Filter.ToFilter(),
            PaginationInfo = request.PaginationInfo.ToPaginationInfo()
        };
    }

    public static Proto.V1SearchCoursesResponse ToV1SearchCoursesResponse(
        this SearchCoursesQueryResponse queryResponse)
    {
        return new Proto.V1SearchCoursesResponse
        {
            CourseInfos = { queryResponse.CourseInfos.ToArrayBy(courseInfo => courseInfo.ToProto()) }
        };
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
            CourseInfos = { queryResponse.CourseInfos.ToArrayBy(courseInfo => courseInfo.ToProto()) }
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
            CourseInfos = { queryResponse.CourseInfos.ToArrayBy(courseInfo => courseInfo.ToProto()) }
        };
    }

    private static Proto.CourseInfo ToProto(this SearchCoursesQueryResponseItem queryResponseItem)
    {
        var course = queryResponseItem.Course;

        return new Proto.CourseInfo
        {
            Id = (long)course.Id,
            Name = course.Name,
            Description = course.Description,
            Status = course.Status.ToProto(),
            StudentCount = queryResponseItem.StudentCount,
            HomeworkCount = queryResponseItem.HomeworkCount
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
