using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;
using Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;
using Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;
using Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Participants;
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

    public static BulkAddGroupStudentsCommand ToBulkAddGroupStudentsCommand(this V1BulkAddGroupStudentsRequest request)
    {
        return new BulkAddGroupStudentsCommand
        {
            GroupId = new GroupId(request.GroupId),
            StudentIds = request.StudentIds.ToArrayBy(studentId => new StudentId(studentId)),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static V1BulkAddGroupStudentsResponse ToV1BulkAddGroupStudentsResponse(
        this CommandResponse<BulkAddGroupStudentsCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new V1BulkAddGroupStudentsResponse
            {
                SuccessResponse = new V1BulkAddGroupStudentsResponse.Types.Success
                {
                    AddedStudentIds = { success.AddedStudentIds.ToArrayBy(studentId => (long)studentId) },
                    SkippedStudentInfos = { success.SkippedStudents.ToArrayBy(skippedItem => skippedItem.ToProto()) }
                }
            },
            validationError => new V1BulkAddGroupStudentsResponse
            {
                ValidationError = validationError.ToProto<BulkAddGroupStudentsCommand, V1BulkAddGroupStudentsRequest>()
            },
            otherError => new V1BulkAddGroupStudentsResponse { OtherError = otherError.ToProto() });
    }

    public static AddGroupTeacherCommand ToAddGroupTeacherCommand(this V1AddGroupTeacherRequest request)
    {
        return new AddGroupTeacherCommand
        {
            GroupId = new GroupId(request.GroupId),
            TeacherId = new TeacherId(request.TeacherId),
            ActorTeacherId = new TeacherId(request.ActorTeacherId)
        };
    }

    public static V1AddGroupTeacherResponse ToV1AddGroupTeacherResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new V1AddGroupTeacherResponse
            {
                SuccessResponse = new V1AddGroupTeacherResponse.Types.Success()
            },
            validationError => new V1AddGroupTeacherResponse
            {
                ValidationError = validationError.ToProto<AddGroupTeacherCommand, V1AddGroupTeacherRequest>()
            },
            otherError => new V1AddGroupTeacherResponse { OtherError = otherError.ToProto() });
    }

    private static TeacherInfo ToTeacherInfo(this Teacher teacher)
    {
        return new TeacherInfo
        {
            TeacherId = (long)teacher.Id,
            Email = teacher.Email,
            Name = teacher.Name
        };
    }

    private static StudentInfo ToStudentInfo(this Student student)
    {
        return new StudentInfo
        {
            StudentId = (long)student.Id,
            Email = student.Email,
            Name = student.Name
        };
    }

    private static V1BulkAddGroupStudentsResponse.Types.SkippedStudentInfo ToProto(
        this SkippedStudentInfo skippedStudentInfo)
    {
        return new V1BulkAddGroupStudentsResponse.Types.SkippedStudentInfo
        {
            StudentId = (long)skippedStudentInfo.Id,
            Reason = skippedStudentInfo.Reason.ToProto()
        };
    }

    private static V1BulkAddGroupStudentsResponse.Types.SkipReason ToProto(
        this SkippedStudentReason skipReason)
    {
        return skipReason switch
        {
            SkippedStudentReason.AlreadyInGroup => V1BulkAddGroupStudentsResponse.Types.SkipReason.AlreadyInGroup,
            SkippedStudentReason.NotFound => V1BulkAddGroupStudentsResponse.Types.SkipReason.NotFound,
            _ => V1BulkAddGroupStudentsResponse.Types.SkipReason.Unspecified
        };
    }
}
