using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

internal sealed class GetStudentHomeworkHandler : IQueryHandler<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IQueryValidator<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse> _validator;

    public GetStudentHomeworkHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IQueryValidator<GetStudentHomeworkQuery, GetStudentHomeworkQueryResponse> validator)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _validator = validator;
    }

    public async Task<GetStudentHomeworkQueryResponse> ExecuteAsync(
        GetStudentHomeworkQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);
        var homeworkStudent = query.ToHomeworkStudent(query.HomeworkId);
        var studentHomework = await unitOfWork.ReadOnlyHomeworkRepository.GetStudentHomeworkInfoAsync(homeworkStudent, cancellationToken);
        var files = await unitOfWork.ReadOnlyHomeworkFileRepository.ListFilesAsync(query.HomeworkId, cancellationToken);

        var submittedHomeworkId = studentHomework!.IsHomeworkSubmitted
            ? await unitOfWork.ReadOnlySubmittedHomeworkRepository.GetSubmittedHomeworkIdAsync(homeworkStudent, cancellationToken)
            : null;

        return new GetStudentHomeworkQueryResponse
        {
            StudentHomeworkInfo = studentHomework,
            Files = files,
            SubmittedHomeworkId = submittedHomeworkId
        };
    }
}
