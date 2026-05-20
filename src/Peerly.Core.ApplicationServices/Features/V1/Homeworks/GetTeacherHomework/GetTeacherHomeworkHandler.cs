using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

internal sealed class GetTeacherHomeworkHandler : IQueryHandler<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse> _validator;

    public GetTeacherHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IQueryValidator<GetTeacherHomeworkQuery, GetTeacherHomeworkQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<GetTeacherHomeworkQueryResponse> ExecuteAsync(GetTeacherHomeworkQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var teacherHomeworkInfo = await unitOfWork.ReadOnlyHomeworkRepository.GetTeacherHomeworkInfoAsync(query.HomeworkId, cancellationToken);
        var files = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFilesAsync(query.HomeworkId, cancellationToken);

        var submittedHomeworkCount = teacherHomeworkInfo!.Status != HomeworkStatus.Draft
            ? (int?)await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetCountAsync(query.HomeworkId, cancellationToken)
            : null;

        return new GetTeacherHomeworkQueryResponse
        {
            TeacherHomeworkInfo = teacherHomeworkInfo,
            SubmittedHomeworkCount = submittedHomeworkCount,
            Files = files
        };
    }
}
