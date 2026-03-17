using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;
using Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Courses;

[ExcludeFromCodeCoverage]
public sealed class CourseController : CourseService.CourseServiceBase
{
    private readonly IQueryHandler<SearchCoursesQuery, SearchCoursesQueryResponse> _searchCoursesHandler;
    private readonly IQueryHandler<SearchStudentCoursesQuery, SearchStudentCoursesQueryResponse> _searchStudentCoursesHandler;
    private readonly IQueryHandler<SearchTeacherCoursesQuery, SearchTeacherCoursesQueryResponse> _searchTeacherCoursesHandler;
    private readonly ICommandHandler<CreateCourseCommand, Success> _createCourseHandler;
    private readonly ICommandHandler<DeleteCourseCommand, Success> _deleteCourseHandler;
    private readonly ICommandHandler<UpdateCourseCommand, Success> _updateCourseHandler;
    private readonly IQueryHandler<GetTeacherCourseQuery, GetTeacherCourseQueryResponse> _getTeacherCourseHandler;
    private readonly IQueryHandler<GetStudentCourseQuery, GetStudentCourseQueryResponse> _getStudentCourseHandler;

    public CourseController(
        IQueryHandler<SearchCoursesQuery, SearchCoursesQueryResponse> searchCoursesHandler,
        IQueryHandler<SearchStudentCoursesQuery, SearchStudentCoursesQueryResponse> searchStudentCoursesHandler,
        IQueryHandler<SearchTeacherCoursesQuery, SearchTeacherCoursesQueryResponse> searchTeacherCoursesHandler,
        ICommandHandler<CreateCourseCommand, Success> createCourseHandler,
        ICommandHandler<DeleteCourseCommand, Success> deleteCourseHandler,
        ICommandHandler<UpdateCourseCommand, Success> updateCourseHandler,
        IQueryHandler<GetTeacherCourseQuery, GetTeacherCourseQueryResponse> getTeacherCourseHandler,
        IQueryHandler<GetStudentCourseQuery, GetStudentCourseQueryResponse> getStudentCourseHandler)
    {
        _searchCoursesHandler = searchCoursesHandler;
        _searchStudentCoursesHandler = searchStudentCoursesHandler;
        _searchTeacherCoursesHandler = searchTeacherCoursesHandler;
        _createCourseHandler = createCourseHandler;
        _deleteCourseHandler = deleteCourseHandler;
        _updateCourseHandler = updateCourseHandler;
        _getTeacherCourseHandler = getTeacherCourseHandler;
        _getStudentCourseHandler = getStudentCourseHandler;
    }

    public override async Task<V1GetTeacherCourseResponse> V1GetTeacherCourse(V1GetTeacherCourseRequest request, ServerCallContext context)
    {
        var query = request.ToGetTeacherCourseQuery();
        var queryResponse = await _getTeacherCourseHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetTeacherCourseResponse();
    }

    public override async Task<V1GetStudentCourseResponse> V1GetStudentCourse(V1GetStudentCourseRequest request, ServerCallContext context)
    {
        var query = request.ToGetStudentCourseQuery();
        var queryResponse = await _getStudentCourseHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetStudentCourseResponse();
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

    // todo: поправить ручку
    public override async Task<V1SearchStudentCoursesResponse> V1SearchStudentCourses(
        V1SearchStudentCoursesRequest request,
        ServerCallContext context)
    {
        var query = request.ToSearchStudentCoursesQuery();
        var queryResponse = await _searchStudentCoursesHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchStudentCoursesResponse();
    }

    // todo: поправить ручку
    public override async Task<V1SearchTeacherCoursesResponse> V1SearchTeacherCourses(
        V1SearchTeacherCoursesRequest request,
        ServerCallContext context)
    {
        var query = request.ToSearchTeacherCoursesQuery();
        var queryResponse = await _searchTeacherCoursesHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchTeacherCoursesResponse();
    }

    public override async Task<V1DeleteCourseResponse> V1DeleteCourse(V1DeleteCourseRequest request, ServerCallContext context)
    {
        var command = request.ToDeleteCourseCommand();
        var responseCommand = await _deleteCourseHandler.ExecuteAsync(command, context.CancellationToken);
        return responseCommand.ToV1DeleteCourseResponse();
    }

    public override async Task<V1UpdateCourseResponse> V1UpdateCourse(V1UpdateCourseRequest request, ServerCallContext context)
    {
        var command = request.ToUpdateCourseCommand();
        var responseCommand = await _updateCourseHandler.ExecuteAsync(command, context.CancellationToken);
        return responseCommand.ToV1UpdateCourseResponse();
    }
}
