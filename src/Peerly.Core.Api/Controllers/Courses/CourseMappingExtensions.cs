using System;
using System.Diagnostics.CodeAnalysis;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;
using Peerly.Core.Models.Courses;
using Peerly.Core.Pagination;
using Peerly.Core.Tools;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Courses;

[ExcludeFromCodeCoverage]
internal static class CourseMappingExtensions
{
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

    private static Proto.V1SearchCoursesResponse.Types.CourseInfo ToProto(this SearchCoursesQueryResponseItem queryResponseItem)
    {
        var course = queryResponseItem.Course;

        return new Proto.V1SearchCoursesResponse.Types.CourseInfo
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Status = course.Status.ToProto(),
            StudentCount = queryResponseItem.StudentCount,
            HomeworkCount = queryResponseItem.HomeworkCount
        };
    }

    private static SearchCoursesQueryFilter ToFilter(this Proto.V1SearchCoursesRequest.Types.Filter filterProto)
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
