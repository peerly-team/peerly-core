using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse.Abstractions;

internal interface ICreateCourseHandlerMapper
{
    CourseAddItem ToCourseAddItem(CreateCourseCommand command);
    CourseTeacherAddItem ToCourseTeacherAddItem(CreateCourseCommand command, CourseId courseId);
}
