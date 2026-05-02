using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

internal sealed class GetTeacherCourseQueryValidator : IQueryValidator<GetTeacherCourseQuery, GetTeacherCourseQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public GetTeacherCourseQueryValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task ValidateAsync(GetTeacherCourseQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var courseTeacher = query.ToCourseTeacher();
        var isCourseTeacherExists = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken) ||
                                    await unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(courseTeacher, cancellationToken);
        if (!isCourseTeacherExists)
        {
            throw new NotFoundException();
        }

        var isCourseExists = await unitOfWork.ReadOnlyCourseRepository.ExistsAsync(query.CourseId, cancellationToken);
        if (!isCourseExists)
        {
            throw new NotFoundException();
        }
    }
}
