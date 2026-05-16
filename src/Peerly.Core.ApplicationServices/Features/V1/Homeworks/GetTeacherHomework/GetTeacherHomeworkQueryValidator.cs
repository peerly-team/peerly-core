using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

internal sealed class GetTeacherHomeworkQueryValidator : IQueryValidator<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public GetTeacherHomeworkQueryValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task ValidateAsync(GetTeacherHomeworkQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);
        if (homework is null)
        {
            throw new NotFoundException();
        }

        var courseTeacher = query.ToCourseTeacher(homework.CourseId);
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacher, cancellationToken);
        if (!isCourseTeacher)
        {
            var isGroupTeacher = await unitOfWork.ReadOnlyGroupTeacherRepository.ExistsAsync(courseTeacher, cancellationToken);
            if (!isGroupTeacher)
            {
                throw new NotFoundException();
            }
        }
    }
}
