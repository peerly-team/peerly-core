using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;

internal sealed class CreateGroupHomeworkHandlerMapper : ICreateGroupHomeworkHandlerMapper
{
    private readonly IClock _clock;

    public CreateGroupHomeworkHandlerMapper(IClock clock)
    {
        _clock = clock;
    }

    public GroupFilter ToGroupFilter(GroupId groupId)
    {
        return new GroupFilter
        {
            GroupIds = [groupId],
            CourseIds = []
        };
    }

    public HomeworkAddItem ToHomeworkAddItem(CreateGroupHomeworkCommand command, CourseId courseId)
    {
        return new HomeworkAddItem
        {
            CourseId = courseId,
            GroupId = command.GroupId,
            TeacherId = command.TeacherId,
            Name = command.Name,
            Status = HomeworkStatus.Draft,
            AmountOfReviewers = command.AmountOfReviewers,
            Description = command.Description,
            Checklist = command.Checklist,
            Deadline = command.Deadline,
            ReviewDeadline = command.ReviewDeadline,
            DiscrepancyThreshold = command.DiscrepancyThreshold,
            CreationTime = _clock.GetCurrentTime()
        };
    }
}
