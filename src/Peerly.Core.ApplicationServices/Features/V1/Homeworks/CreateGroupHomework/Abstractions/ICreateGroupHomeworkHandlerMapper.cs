using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework.Abstractions;

internal interface ICreateGroupHomeworkHandlerMapper
{
    GroupFilter ToGroupFilter(GroupId groupId);
    HomeworkAddItem ToHomeworkAddItem(CreateGroupHomeworkCommand command, CourseId courseId);
}
