using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Persistence.Repositories.GroupStudents.Models;

namespace Peerly.Core.Persistence.Repositories.GroupStudents;

internal static class GroupStudentRepositoryMapper
{
    public static GroupStudent ToGroupStudent(this GroupStudentDb groupStudentDb)
    {
        return new GroupStudent
        {
            GroupId = new GroupId(groupStudentDb.GroupId),
            StudentId = new StudentId(groupStudentDb.StudentId)
        };
    }
}
