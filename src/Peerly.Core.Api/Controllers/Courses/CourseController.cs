using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Courses;

[ExcludeFromCodeCoverage]
public sealed class CourseController : CourseService.CourseServiceBase
{
    private readonly IQueryHandler<SearchCoursesQuery, SearchCoursesQueryResponse> _searchCoursesHandler;
    private readonly IQueryHandler<SearchStudentCoursesQuery, SearchStudentCoursesQueryResponse> _searchStudentCoursesHandler;
    private readonly IQueryHandler<SearchTeacherCoursesQuery, SearchTeacherCoursesQueryResponse> _searchTeacherCoursesHandler;
    private readonly ICommandHandler<CreateCourseCommand, Success> _createCourseHandler;

    public CourseController(
        IQueryHandler<SearchCoursesQuery, SearchCoursesQueryResponse> searchCoursesHandler,
        IQueryHandler<SearchStudentCoursesQuery, SearchStudentCoursesQueryResponse> searchStudentCoursesHandler,
        IQueryHandler<SearchTeacherCoursesQuery, SearchTeacherCoursesQueryResponse> searchTeacherCoursesHandler,
        ICommandHandler<CreateCourseCommand, Success> createCourseHandler)
    {
        _searchCoursesHandler = searchCoursesHandler;
        _searchStudentCoursesHandler = searchStudentCoursesHandler;
        _searchTeacherCoursesHandler = searchTeacherCoursesHandler;
        _createCourseHandler = createCourseHandler;
    }

    public override async Task<V1CreateCourseResponse> V1CreateCourse(V1CreateCourseRequest request, ServerCallContext context)
    {
        var command = request.ToCreateCourseCommand();
        var responseCommand = await _createCourseHandler.ExecuteAsync(command, context.CancellationToken);
        return responseCommand.ToV1CreateCourseResponse();
    }

    public override async Task<V1SearchCoursesResponse> V1SearchCourses(V1SearchCoursesRequest request, ServerCallContext context)
    {
        var query = request.ToSearchCoursesQuery();
        var queryResponse = await _searchCoursesHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchCoursesResponse();
    }

    public override async Task<V1SearchStudentCoursesResponse> V1SearchStudentCourses(
        V1SearchStudentCoursesRequest request,
        ServerCallContext context)
    {
        var query = request.ToSearchStudentCoursesQuery();
        var queryResponse = await _searchStudentCoursesHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchStudentCoursesResponse();
    }

    public override async Task<V1SearchTeacherCoursesResponse> V1SearchTeacherCourses(V1SearchTeacherCoursesRequest request, ServerCallContext context)
    {
        var query = request.ToSearchTeacherCoursesQuery();
        var queryResponse = await _searchTeacherCoursesHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchTeacherCoursesResponse();
    }
}
