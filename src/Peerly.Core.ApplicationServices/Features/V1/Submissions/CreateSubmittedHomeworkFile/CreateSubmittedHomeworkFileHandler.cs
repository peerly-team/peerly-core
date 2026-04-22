using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.Abstractions;
using Peerly.Core.ApplicationServices.Features.Validations;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.ApplicationServices.Services.Anonymization.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Mapper = Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.CreateSubmittedHomeworkFileHandlerMapper;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

// todo: выполнить рефакторинг
internal sealed class CreateSubmittedHomeworkFileHandler : ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IFileAnonymizationService _anonymizationService;
    private readonly ICreateSubmittedHomeworkFileValidator _createSubmittedHomeworkFileValidator;
    private readonly IClock _clock;

    public CreateSubmittedHomeworkFileHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IFileAnonymizationService anonymizationService,
        ICreateSubmittedHomeworkFileValidator createSubmittedHomeworkFileValidator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _anonymizationService = anonymizationService;
        _createSubmittedHomeworkFileValidator = createSubmittedHomeworkFileValidator;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateSubmittedHomeworkFileCommandResponse>> ExecuteAsync(
        CreateSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var validationError = await _createSubmittedHomeworkFileValidator.RunAsync(command, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var submittedHomework = await unitOfWork.SubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        var homework = await unitOfWork.HomeworkRepository.GetAsync(submittedHomework!.HomeworkId, cancellationToken);
        if (homework is null)
        {
            return OtherError.NotFound(HomeworkErrors.HomeworkNotFound);
        }

        // todo: добавить анонимизацию названия файла, чтобы не было случаев, когда пользователь назвал файл своим именем
        var students = await GetStudentsAsync(unitOfWork, homework, cancellationToken);
        var anonymizationResponse = await _anonymizationService.AnonymizeAsync(command.ToAnonymizationItem(students), cancellationToken);

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var creationTime = _clock.GetCurrentTime();
        var fileId = await unitOfWork.FileRepository.AddAsync(command.ToFileAddItem(creationTime), cancellationToken);

        var anonymizedFileId = anonymizationResponse is null
            ? (FileId?)null
            : await unitOfWork.FileRepository.AddAsync(
                command.ToAnonymizedFileAddItem(anonymizationResponse, creationTime),
                cancellationToken);

        await unitOfWork.SubmittedHomeworkFileRepository.AddAsync(
            command.ToSubmittedHomeworkFileAddItem(fileId, anonymizedFileId),
            cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateSubmittedHomeworkFileCommandResponse
        {
            FileId = fileId
        };
    }

    private static async Task<IReadOnlyCollection<Student>> GetStudentsAsync(
        ICommonUnitOfWork unitOfWork,
        Homework homework,
        CancellationToken cancellationToken)
    {
        var groupStudentFilter = homework.GroupId is not null
            ? Mapper.ToGroupStudentFilter(homework.GroupId.Value)
            : await GetCourseGroupStudentFilterAsync(unitOfWork, homework.CourseId, cancellationToken);
        var groupStudents = await unitOfWork.GroupStudentRepository.ListAsync(groupStudentFilter, cancellationToken);

        return await unitOfWork.StudentRepository.ListAsync(Mapper.ToStudentFilter(groupStudents), cancellationToken);
    }

    private static async Task<GroupStudentFilter> GetCourseGroupStudentFilterAsync(
        ICommonUnitOfWork unitOfWork,
        CourseId courseId,
        CancellationToken cancellationToken)
    {
        var groups = await unitOfWork.GroupRepository.ListAsync(Mapper.ToGroupFilter(courseId), cancellationToken);
        return Mapper.ToGroupStudentFilter(groups);
    }
}
