using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.GetHomework;
using Peerly.Core.Exceptions;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

internal sealed class GetTeacherHomeworkHandler : IQueryHandler<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;

    public GetTeacherHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
    }

    public async Task<GetTeacherHomeworkQueryResponse> ExecuteAsync(
        GetTeacherHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var homework = await unitOfWork.ReadOnlyHomeworkRepository.GetAsync(query.HomeworkId, cancellationToken);
        if (homework is null)
        {
            throw new NotFoundException();
        }

        var courseTeacherExistsItem = new CourseTeacherExistsItem
        {
            CourseId = homework.CourseId,
            TeacherId = query.TeacherId
        };
        var isCourseTeacher = await unitOfWork.ReadOnlyCourseTeacherRepository.ExistsAsync(courseTeacherExistsItem, cancellationToken);
        if (!isCourseTeacher)
        {
            throw new NotFoundException();
        }

        var fileIds = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFileIdsAsync(query.HomeworkId, cancellationToken);
        var files = fileIds.Count > 0
            ? await unitOfWork.ReadOnlyFileRepository.ListByIdsAsync(fileIds, cancellationToken)
            : [];

        return new GetTeacherHomeworkQueryResponse
        {
            HomeworkDetail = new HomeworkDetailResponseItem
            {
                Homework = homework,
                Files = files
            }
        };
    }
}
