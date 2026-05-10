using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.ApplicationServices.Services.Anonymization.Abstractions;
using Peerly.Core.ApplicationServices.Services.Anonymization.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Models.Students;
using Mapper = Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile.CreateSubmittedHomeworkFileHandlerMapper;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

internal sealed class CreateSubmittedHomeworkFileHandler : ICommandHandler<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly IFileAnonymizationService _anonymizationService;
    private readonly ICommandValidator<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> _validator;
    private readonly IClock _clock;

    public CreateSubmittedHomeworkFileHandler(
        ICommonUnitOfWorkFactory commonUnitOfWorkFactory,
        IFileAnonymizationService anonymizationService,
        ICommandValidator<CreateSubmittedHomeworkFileCommand, CreateSubmittedHomeworkFileCommandResponse> validator,
        IClock clock)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _anonymizationService = anonymizationService;
        _validator = validator;
        _clock = clock;
    }

    public async Task<CommandResponse<CreateSubmittedHomeworkFileCommandResponse>> ExecuteAsync(
        CreateSubmittedHomeworkFileCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var submittedHomework = await unitOfWork.SubmittedHomeworkRepository.GetAsync(command.SubmittedHomeworkId, cancellationToken);
        var homework = await unitOfWork.HomeworkRepository.GetAsync(submittedHomework!.HomeworkId, cancellationToken);
        var students = await GetStudentsAsync(unitOfWork, homework!, cancellationToken);

        var anonymizationResult = await _anonymizationService.AnonymizeAsync(command.ToAnonymizationItem(students), cancellationToken);

        await using var operationSet = await unitOfWork.StartOperationSet(cancellationToken);

        var creationTime = _clock.GetCurrentTime();
        var fileId = await unitOfWork.FileRepository.AddAsync(command.ToFileAddItem(creationTime), cancellationToken);

        var anonymizedFileId = await AddAnonymizeFileIfNeedAsync(unitOfWork, anonymizationResult, creationTime, cancellationToken);
        await unitOfWork.SubmittedHomeworkFileRepository.AddAsync(command.ToSubmittedHomeworkFileAddItem(fileId, anonymizedFileId), cancellationToken);

        await operationSet.Complete(cancellationToken);

        return new CreateSubmittedHomeworkFileCommandResponse
        {
            FileId = fileId
        };
    }

    private static async Task<FileId?> AddAnonymizeFileIfNeedAsync(
        ICommonUnitOfWork unitOfWork,
        AnonymizationResult? anonymizationResult,
        DateTimeOffset creationTime,
        CancellationToken cancellationToken)
    {
        if (anonymizationResult is null)
        {
            return null;
        }

        return await unitOfWork.FileRepository.AddAsync(
            anonymizationResult.ToAnonymizedFileAddItem(creationTime),
            cancellationToken);
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
