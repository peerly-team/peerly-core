using System;
using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Students.GetStudent;
using Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;
using Peerly.Core.ApplicationServices.Features.V1.Teachers.GetTeacher;
using Peerly.Core.ApplicationServices.Features.V1.Teachers.UpdateTeacher;
using Peerly.Core.ApplicationServices.Features.V1.Users.SearchUsers;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Users;
using Peerly.Core.Tools;
using Peerly.Core.V1;
using Proto = Peerly.Core.V1.V1SearchUsersResponse.Types;

namespace Peerly.Core.Api.Controllers.Users;

internal static class UserControllerMapper
{
    public static SearchUsersQuery ToSearchUsersQuery(this V1SearchUsersRequest request)
    {
        return new SearchUsersQuery
        {
            Filter = new UserFilter
            {
                Query = request.Filter.Query,
                Roles = request.Filter.Roles.ToArrayBy(role => role.ToModel()),
                Limit = request.Limit
            }
        };
    }

    public static V1SearchUsersResponse ToV1SearchUsersResponse(this SearchUsersQueryResponse queryResponse)
    {
        return new V1SearchUsersResponse
        {
            Users = { queryResponse.Users.ToArrayBy(user => user.ToUserInfo()) }
        };
    }

    private static Proto.UserInfo ToUserInfo(this User user)
    {
        return new Proto.UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToProto()
        };
    }

    private static UserRole ToModel(this Role roleProto)
    {
        return roleProto switch
        {
            Role.Teacher => UserRole.Teacher,
            Role.Student => UserRole.Student,
            _ => throw new ArgumentOutOfRangeException(nameof(roleProto), roleProto, null)
        };
    }

    private static Role ToProto(this UserRole role)
    {
        return role switch
        {
            UserRole.Teacher => Role.Teacher,
            UserRole.Student => Role.Student,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }

    public static GetStudentQuery ToGetStudentQuery(this V1GetStudentRequest request)
    {
        return new GetStudentQuery
        {
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static V1GetStudentResponse ToV1GetStudentResponse(this GetStudentQueryResponse queryResponse)
    {
        return new V1GetStudentResponse
        {
            StudentInfo = new StudentInfo
            {
                StudentId = (long)queryResponse.Student.Id,
                Email = queryResponse.Student.Email,
                Name = queryResponse.Student.Name
            }
        };
    }

    public static GetTeacherQuery ToGetTeacherQuery(this V1GetTeacherRequest request)
    {
        return new GetTeacherQuery
        {
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static V1GetTeacherResponse ToV1GetTeacherResponse(this GetTeacherQueryResponse queryResponse)
    {
        return new V1GetTeacherResponse
        {
            TeacherInfo = new TeacherInfo
            {
                TeacherId = (long)queryResponse.Teacher.Id,
                Email = queryResponse.Teacher.Email,
                Name = queryResponse.Teacher.Name
            }
        };
    }

    public static UpdateStudentCommand ToUpdateStudentCommand(this V1UpdateStudentRequest request)
    {
        return new UpdateStudentCommand
        {
            StudentId = new StudentId(request.StudentId),
            Name = request.Name
        };
    }

    public static V1UpdateStudentResponse ToV1UpdateStudentResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new V1UpdateStudentResponse
            {
                SuccessResponse = new V1UpdateStudentResponse.Types.Success()
            },
            validationError => new V1UpdateStudentResponse
            {
                ValidationError = validationError.ToProto<UpdateStudentCommand, V1UpdateStudentRequest>()
            },
            otherError => new V1UpdateStudentResponse { OtherError = otherError.ToProto() });
    }

    public static UpdateTeacherCommand ToUpdateTeacherCommand(this V1UpdateTeacherRequest request)
    {
        return new UpdateTeacherCommand
        {
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name
        };
    }

    public static V1UpdateTeacherResponse ToV1UpdateTeacherResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new V1UpdateTeacherResponse
            {
                SuccessResponse = new V1UpdateTeacherResponse.Types.Success()
            },
            validationError => new V1UpdateTeacherResponse
            {
                ValidationError = validationError.ToProto<UpdateTeacherCommand, V1UpdateTeacherRequest>()
            },
            otherError => new V1UpdateTeacherResponse { OtherError = otherError.ToProto() });
    }
}
