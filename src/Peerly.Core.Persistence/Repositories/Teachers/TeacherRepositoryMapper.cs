using Peerly.Core.Identifiers;
using Peerly.Core.Models.Teachers;
using Peerly.Core.Persistence.Repositories.Teachers.Models;

namespace Peerly.Core.Persistence.Repositories.Teachers;

internal static class TeacherRepositoryMapper
{
    public static Teacher ToTeacher(this TeacherDb db)
    {
        return new Teacher
        {
            Id = new TeacherId(db.Id),
            Email = db.Email,
            Name = db.Name
        };
    }
}
