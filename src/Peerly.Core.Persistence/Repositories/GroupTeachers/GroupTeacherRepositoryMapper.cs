using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Persistence.Repositories.GroupTeachers.Models;

namespace Peerly.Core.Persistence.Repositories.GroupTeachers;

internal static class GroupTeacherRepositoryMapper
{
    public static GroupTeacher ToGroupTeacher(this GroupTeacherDb groupTeacherDb)
    {
        return new GroupTeacher
        {
            GroupId = new GroupId(groupTeacherDb.GroupId),
            TeacherId = new TeacherId(groupTeacherDb.TeacherId)
        };
    }
}
