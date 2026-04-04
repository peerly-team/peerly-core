using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.ApplicationServices.Services.Anonymization.Abstractions;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;
using Mapper = Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.CreateSubmittedHomeworkFileHandlerMapper;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

internal sealed class
    CreateSubmittedHomeworkFileHandler : ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IFileAnonymizationService _anonymizationService;
    private readonly ICreateSubmittedHomeworkFileHandlerMapper _mapper;

    public CreateSubmittedHomeworkFileHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IFileAnonymizationService anonymizationService,
        ICreateSubmittedHomeworkFileHandlerMapper mapper)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _anonymizationService = anonymizationService;
        _mapper = mapper;
    }

    public async Task<CommandResponse<CreateSubmittedHomeworkFileCommandResponse>> ExecuteAsync(
        CreateSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var anonymizationResponse = await TryAnonymizeAsync(unitOfWork, command, cancellationToken);

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var fileAddItem = _mapper.ToFileAddItem(command);
        var fileId = await unitOfWork.FileRepository.AddAsync(fileAddItem, cancellationToken);

        var anonymizedFileId = anonymizationResponse is null
            ? (FileId?)null
            : await unitOfWork.FileRepository.AddAsync(_mapper.ToAnonymizedFileAddItem(command, anonymizationResponse), cancellationToken);

        var submittedHomeworkFileAddItem = new SubmittedHomeworkFileAddItem
        {
            SubmittedHomeworkId = command.SubmittedHomeworkId,
            FileId = fileId,
            AnonymizedFileId = anonymizedFileId
        };
        await unitOfWork.SubmittedHomeworkFileRepository.AddAsync(submittedHomeworkFileAddItem, cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateSubmittedHomeworkFileCommandResponse
        {
            FileId = fileId
        };
    }

    private async Task<AnonymizationResponse?> TryAnonymizeAsync(
        ICommonUnitOfWork unitOfWork,
        CreateSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        var submittedHomework = await unitOfWork.SubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        if (submittedHomework is null)
        {
            return null;
        }

        var homework = await unitOfWork.HomeworkRepository.GetAsync(submittedHomework.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return null;
        }

        var students = await GetStudentsAsync(unitOfWork, homework, cancellationToken);
        var anonymizationItem = Mapper.ToAnonymizationItem(command, students);
        return await _anonymizationService.AnonymizeAsync(anonymizationItem, cancellationToken);
    }

    private async Task<IReadOnlyCollection<Student>> GetStudentsAsync(
        ICommonUnitOfWork unitOfWork,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var groupStudentFilter = homework.GroupId is not null
            ? Mapper.ToGroupStudentFilter(homework.GroupId.Value)
            : await GetCourseGroupStudentFilterAsync(unitOfWork, homework.CourseId, cancellationToken);
        var groupStudents = await unitOfWork.GroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);

        var studentFilter = Mapper.ToStudentFilter(groupStudents);
        return await unitOfWork.StudentRepository.ListAsync(studentFilter, cancellationToken);
    }

    private async Task<GroupStudentFilter> GetCourseGroupStudentFilterAsync(
        ICommonUnitOfWork unitOfWork,
        CourseId courseId,
        CancellationToken cancellationToken)
    {
        var groupFilter = Mapper.ToGroupFilter(courseId);
        var groups = await unitOfWork.GroupRepository.ListAsync(groupFilter, cancellationToken);
        return Mapper.ToGroupStudentFilter(groups);
    }
}
