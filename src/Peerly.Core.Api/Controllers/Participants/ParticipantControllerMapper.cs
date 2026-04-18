using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupStudent;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Teachers;
using Peerly.Core.Tools;
using Peerly.Core.V1;
using Student = Peerly.Core.Models.Students.Student;

namespace Peerly.Core.Api.Controllers.Participants;

internal static class ParticipantControllerMapper
{
    public static ListCourseParticipantsQuery ToListCourseParticipantsQuery(this V1ListCourseParticipantsRequest request)
    {
        return new ListCourseParticipantsQuery
        {
            CourseId = new CourseId(request.CourseId)
        };
    }

    public static V1ListCourseParticipantsResponse ToV1ListCourseParticipantsResponse(
        this ListCourseParticipantsQueryResponse queryResponse)
    {
        return new V1ListCourseParticipantsResponse
        {
            Teachers = { queryResponse.Teachers.ToArrayBy(teacher => teacher.ToTeacherInfo()) },
            Students = { queryResponse.Students.ToArrayBy(student => student.ToStudentInfo()) }
        };
    }

    public static ListGroupParticipantsQuery ToListGroupParticipantsQuery(this V1ListGroupParticipantsRequest request)
    {
        return new ListGroupParticipantsQuery
        {
            GroupId = new GroupId(request.GroupId)
        };
    }

    public static V1ListGroupParticipantsResponse ToV1ListGroupParticipantsResponse(
        this ListGroupParticipantsQueryResponse queryResponse)
    {
        return new V1ListGroupParticipantsResponse
        {
            Teachers = { queryResponse.Teachers.ToArrayBy(teacher => teacher.ToTeacherInfo()) },
            Students = { queryResponse.Students.ToArrayBy(student => student.ToStudentInfo()) }
        };
    }

    public static AddGroupStudentCommand ToAddGroupStudentCommand(this V1AddGroupStudentRequest request)
    {
        return new AddGroupStudentCommand
        {
            GroupId = new GroupId(request.GroupId),
            StudentId = new StudentId(request.StudentId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static V1AddGroupStudentResponse ToV1AddGroupStudentResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new V1AddGroupStudentResponse
            {
                SuccessResponse = new V1AddGroupStudentResponse.Types.Success()
            },
            validationError => new V1AddGroupStudentResponse
            {
                ValidationError = validationError.ToProto<AddGroupStudentCommand, V1AddGroupStudentRequest>()
            },
            otherError => new V1AddGroupStudentResponse { OtherError = otherError.ToProto() });
    }

    private static TeacherInfo ToTeacherInfo(this Teacher teacher)
    {
        return new TeacherInfo
        {
            TeacherId = (long)teacher.Id,
            Email = teacher.Email,
            Name = teacher.Name ?? string.Empty
        };
    }

    private static StudentInfo ToStudentInfo(this Student student)
    {
        return new StudentInfo
        {
            StudentId = (long)student.Id,
            Email = student.Email,
            Name = student.Name ?? string.Empty
        };
    }
}
