using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

internal sealed class GetStudentCourseQueryValidator : IQueryValidator<GetStudentCourseQuery, GetStudentCourseQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public GetStudentCourseQueryValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task ValidateAsync(GetStudentCourseQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var isCourseExists = await unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, cancellationToken);
        if (!isCourseExists)
        {
            throw new NotFoundException();
        }

        var courseStudent = query.ToCourseStudent();
        var isCourseStudentExists = await unitOfWork.ReadOnlyGroupStudentRepository.ExistsAsync(courseStudent, cancellationToken);
        if (!isCourseStudentExists)
        {
            throw new NotFoundException();
        }
    }
}
