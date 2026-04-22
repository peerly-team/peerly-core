using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Groups.CreateGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.DeleteGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.GetStudentGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.GetTeacherGroup;
using Peerly.Core.ApplicationServices.Features.V1.Groups.ListStudentCourseGroups;
using Peerly.Core.ApplicationServices.Features.V1.Groups.ListTeacherCourseGroups;
using Peerly.Core.ApplicationServices.Features.V1.Groups.UpdateGroup;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Tools;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Groups;

internal static class GroupControllerMapper
{
    public static CreateGroupCommand ToCreateGroupCommand(this V1CreateGroupRequest request)
    {
        return new CreateGroupCommand
        {
            CourseId = new CourseId(request.CourseId),
            Name = request.Name,
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static V1CreateGroupResponse ToV1CreateGroupResponse(this CommandResponse<CreateGroupCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new V1CreateGroupResponse
            {
                SuccessResponse = new V1CreateGroupResponse.Types.Success
                {
                    GroupId = (long)success.GroupId
                }
            },
            validationError => new V1CreateGroupResponse
            {
                ValidationError = validationError.ToProto<CreateGroupCommand, V1CreateGroupRequest>()
            },
            otherError => new V1CreateGroupResponse { OtherError = otherError.ToProto() });
    }

    public static UpdateGroupCommand ToUpdateGroupCommand(this V1UpdateGroupRequest request)
    {
        return new UpdateGroupCommand
        {
            GroupId = new GroupId(request.GroupId),
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name
        };
    }

    public static V1UpdateGroupResponse ToV1UpdateGroupResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new V1UpdateGroupResponse
            {
                SuccessResponse = new V1UpdateGroupResponse.Types.Success()
            },
            validationError => new V1UpdateGroupResponse
            {
                ValidationError = validationError.ToProto<UpdateGroupCommand, V1UpdateGroupRequest>()
            },
            otherError => new V1UpdateGroupResponse { OtherError = otherError.ToProto() });
    }

    public static DeleteGroupCommand ToDeleteGroupCommand(this V1DeleteGroupRequest request)
    {
        return new DeleteGroupCommand
        {
            GroupId = new GroupId(request.GroupId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static V1DeleteGroupResponse ToV1DeleteGroupResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new V1DeleteGroupResponse
            {
                SuccessResponse = new V1DeleteGroupResponse.Types.Success()
            },
            validationError => new V1DeleteGroupResponse
            {
                ValidationError = validationError.ToProto<DeleteGroupCommand, V1DeleteGroupRequest>()
            },
            otherError => new V1DeleteGroupResponse { OtherError = otherError.ToProto() });
    }

    public static GetTeacherGroupQuery ToGetTeacherGroupQuery(this V1GetTeacherGroupRequest request)
    {
        return new GetTeacherGroupQuery
        {
            GroupId = new GroupId(request.GroupId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static V1GetTeacherGroupResponse ToV1GetTeacherGroupResponse(this GetTeacherGroupQueryResponse queryResponse)
    {
        return new V1GetTeacherGroupResponse
        {
            GroupInfo = queryResponse.Group.ToGroupInfo()
        };
    }

    public static GetStudentGroupQuery ToGetStudentGroupQuery(this V1GetStudentGroupRequest request)
    {
        return new GetStudentGroupQuery
        {
            GroupId = new GroupId(request.GroupId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static V1GetStudentGroupResponse ToV1GetStudentGroupResponse(this GetStudentGroupQueryResponse queryResponse)
    {
        return new V1GetStudentGroupResponse
        {
            GroupInfo = queryResponse.Group.ToGroupInfo()
        };
    }

    public static ListTeacherCourseGroupsQuery ToListTeacherCourseGroupsQuery(this V1ListTeacherCourseGroupsRequest request)
    {
        return new ListTeacherCourseGroupsQuery
        {
            TeacherId = new TeacherId(request.TeacherId),
            CourseId = new CourseId(request.CourseId)
        };
    }

    public static V1ListTeacherCourseGroupsResponse ToV1ListTeacherCourseGroupsResponse(this ListTeacherCourseGroupsQueryResponse queryResponse)
    {
        return new V1ListTeacherCourseGroupsResponse
        {
            GroupInfos = { queryResponse.Groups.ToArrayBy(group => group.ToGroupInfo()) }
        };
    }

    public static ListStudentCourseGroupsQuery ToListStudentCourseGroupsQuery(this V1ListStudentCourseGroupsRequest request)
    {
        return new ListStudentCourseGroupsQuery
        {
            StudentId = new StudentId(request.StudentId),
            CourseId = new CourseId(request.CourseId)
        };
    }

    public static V1ListStudentCourseGroupsResponse ToV1ListStudentCourseGroupsResponse(this ListStudentCourseGroupsQueryResponse queryResponse)
    {
        return new V1ListStudentCourseGroupsResponse
        {
            GroupInfos = { queryResponse.Groups.ToArrayBy(group => group.ToGroupInfo()) }
        };
    }

    private static GroupInfo ToGroupInfo(this Group group)
    {
        return new GroupInfo
        {
            Id = (long)group.Id,
            Name = group.Name,
            StudentCount = group.StudentCount
        };
    }
}
