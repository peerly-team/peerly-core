using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

internal sealed class
    GetTeacherSubmittedHomeworkQueryValidator : IQueryValidator<GetTeacherSubmittedHomeworkQuery, GetTeacherSubmittedHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public GetTeacherSubmittedHomeworkQueryValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task ValidateAsync(GetTeacherSubmittedHomeworkQuery query, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var submittedHomework = await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetAsync(query.SubmittedHomeworkId, cancellationToken)
                                ?? throw new NotFoundException();

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
        if (homework?.Status is not (HomeworkStatus.Reviewing or HomeworkStatus.Confirmation or HomeworkStatus.Finished))
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
