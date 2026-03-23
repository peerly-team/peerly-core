using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework.Abstractions;
using Peerly.Core.ApplicationServices.Models.Common;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;

internal sealed class CreateGroupHomeworkHandler : ICommandHandler<CreateGroupHomeworkCommand, CreateGroupHomeworkCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _commonUnitOfWorkFactory;
    private readonly ICreateGroupHomeworkHandlerMapper _mapper;

    public CreateGroupHomeworkHandler(ICommonUnitOfWorkFactory commonUnitOfWorkFactory, ICreateGroupHomeworkHandlerMapper mapper)
    {
        _commonUnitOfWorkFactory = commonUnitOfWorkFactory;
        _mapper = mapper;
    }

    public async Task<CommandResponse<CreateGroupHomeworkCommandResponse>> ExecuteAsync(
        CreateGroupHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _commonUnitOfWorkFactory.CreateAsync(cancellationToken);

        var groupFilter = _mapper.ToGroupFilter(command.GroupId);
        var groups = await unitOfWork.GroupRepository.ListAsync(groupFilter, cancellationToken);

        var group = groups.SingleOrDefault();
        if (group is null)
        {
            return OtherError.NotFound();
        }

        // todo: добавить проверку, что препод может добавлять домашку на курс

        var homeworkAddItem = _mapper.ToHomeworkAddItem(command, group.CourseId);
        var homeworkId = await unitOfWork.HomeworkRepository.AddAsync(homeworkAddItem, cancellationToken);

        return new CreateGroupHomeworkCommandResponse
        {
            HomeworkId = homeworkId
        };
    }
}
