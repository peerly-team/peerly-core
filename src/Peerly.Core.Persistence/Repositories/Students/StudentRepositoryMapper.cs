using Peerly.Core.Identifiers;
using Peerly.Core.Models.Students;
using Peerly.Core.Persistence.Repositories.Students.Models;

namespace Peerly.Core.Persistence.Repositories.Students;

internal static class StudentRepositoryMapper
{
    public static Student ToStudent(this StudentDb db)
    {
        return new Student
        {
            Id = new StudentId(db.Id),
            Email = db.Email,
            Name = db.Name
        };
    }
}
