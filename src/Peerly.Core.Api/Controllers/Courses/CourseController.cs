using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Courses;

[ExcludeFromCodeCoverage]
public sealed class CourseController : CourseService.CourseServiceBase
{
    private readonly IQueryHandler<SearchCoursesQuery, SearchCoursesQueryResponse> _searchCoursesHandler;

    public CourseController(IQueryHandler<SearchCoursesQuery, SearchCoursesQueryResponse> searchCoursesHandler)
    {
        _searchCoursesHandler = searchCoursesHandler;
    }

    public override async Task<V1SearchCoursesResponse> V1SearchCourses(V1SearchCoursesRequest request, ServerCallContext context)
    {
        var query = request.ToSearchCoursesQuery();
        var queryResponse = await _searchCoursesHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1SearchCoursesResponse();
    }
}
