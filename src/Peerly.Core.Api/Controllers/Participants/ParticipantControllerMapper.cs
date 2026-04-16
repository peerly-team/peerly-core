using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupParticipant;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Participants;

internal static class ParticipantControllerMapper
{
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
}
