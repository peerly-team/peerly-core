using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupParticipant;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;
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

    public static AddGroupParticipantCommand ToAddGroupParticipantCommand(this V1AddGroupParticipantRequest request)
    {
        return new AddGroupParticipantCommand
        {
            GroupId = new GroupId(request.GroupId),
            StudentId = new StudentId(request.StudentId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static V1AddGroupParticipantResponse ToV1AddGroupParticipantResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new V1AddGroupParticipantResponse
            {
                SuccessResponse = new V1AddGroupParticipantResponse.Types.Success()
            },
            validationError => new V1AddGroupParticipantResponse
            {
                ValidationError = validationError.ToProto<AddGroupParticipantCommand, V1AddGroupParticipantRequest>()
            },
            otherError => new V1AddGroupParticipantResponse { OtherError = otherError.ToProto() });
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
