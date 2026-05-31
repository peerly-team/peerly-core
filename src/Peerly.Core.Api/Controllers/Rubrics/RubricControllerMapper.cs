using OneOf.Types;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.CreateRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.DeleteRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetStudentRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.GetTeacherRubric;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.ListTeacherRubrics;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.Shared.Models;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.UpdateRubric;
using Peerly.Core.ApplicationServices.Models.Common;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Peerly.Core.Tools;
using Proto = Peerly.Core.V1;

namespace Peerly.Core.Api.Controllers.Rubrics;

internal static class RubricControllerMapper
{
    public static CreateRubricCommand ToCreateRubricCommand(this Proto.V1CreateRubricRequest request)
    {
        return new CreateRubricCommand
        {
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name,
            Criteria = request.Criteria.ToArrayBy(ToCreateRubricCriterionItem)
        };
    }

    public static Proto.V1CreateRubricResponse ToV1CreateRubricResponse(
        this CommandResponse<CreateRubricCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1CreateRubricResponse
            {
                SuccessResponse = new Proto.V1CreateRubricResponse.Types.Success
                {
                    RubricId = (long)success.RubricId
                }
            },
            validationError => new Proto.V1CreateRubricResponse
            {
                ValidationError = validationError.ToProto<CreateRubricCommand, Proto.V1CreateRubricRequest>()
            },
            otherError => new Proto.V1CreateRubricResponse { OtherError = otherError.ToProto() });
    }

    public static UpdateRubricCommand ToUpdateRubricCommand(this Proto.V1UpdateRubricRequest request)
    {
        return new UpdateRubricCommand
        {
            RubricId = new RubricId(request.RubricId),
            TeacherId = new TeacherId(request.TeacherId),
            Name = request.Name,
            Criteria = request.Criteria.ToArrayBy(ToCreateRubricCriterionItem)
        };
    }

    public static Proto.V1UpdateRubricResponse ToV1UpdateRubricResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1UpdateRubricResponse
            {
                SuccessResponse = new Proto.V1UpdateRubricResponse.Types.Success()
            },
            validationError => new Proto.V1UpdateRubricResponse
            {
                ValidationError = validationError.ToProto<UpdateRubricCommand, Proto.V1UpdateRubricRequest>()
            },
            otherError => new Proto.V1UpdateRubricResponse { OtherError = otherError.ToProto() });
    }

    public static DeleteRubricCommand ToDeleteRubricCommand(this Proto.V1DeleteRubricRequest request)
    {
        return new DeleteRubricCommand
        {
            RubricId = new RubricId(request.RubricId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1DeleteRubricResponse ToV1DeleteRubricResponse(
        this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1DeleteRubricResponse
            {
                SuccessResponse = new Proto.V1DeleteRubricResponse.Types.Success()
            },
            validationError => new Proto.V1DeleteRubricResponse
            {
                ValidationError = validationError.ToProto<DeleteRubricCommand, Proto.V1DeleteRubricRequest>()
            },
            otherError => new Proto.V1DeleteRubricResponse { OtherError = otherError.ToProto() });
    }

    public static GetTeacherRubricQuery ToGetTeacherRubricQuery(this Proto.V1GetTeacherRubricRequest request)
    {
        return new GetTeacherRubricQuery
        {
            RubricId = new RubricId(request.RubricId),
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1GetTeacherRubricResponse ToV1GetTeacherRubricResponse(this GetTeacherRubricQueryResponse queryResponse)
    {
        return new Proto.V1GetTeacherRubricResponse
        {
            Rubric = queryResponse.Rubric.ToProto(),
            Criteria = { queryResponse.Criteria.ToArrayBy(ToProto) }
        };
    }

    public static GetStudentRubricQuery ToGetStudentRubricQuery(this Proto.V1GetStudentRubricRequest request)
    {
        return new GetStudentRubricQuery
        {
            RubricId = new RubricId(request.RubricId),
            StudentId = new StudentId(request.StudentId)
        };
    }

    public static Proto.V1GetStudentRubricResponse ToV1GetStudentRubricResponse(this GetStudentRubricQueryResponse queryResponse)
    {
        return new Proto.V1GetStudentRubricResponse
        {
            Criteria = { queryResponse.Criteria.ToArrayBy(ToProto) }
        };
    }

    public static ListTeacherRubricsQuery ToListTeacherRubricsQuery(this Proto.V1ListTeacherRubricsRequest request)
    {
        return new ListTeacherRubricsQuery
        {
            TeacherId = new TeacherId(request.TeacherId)
        };
    }

    public static Proto.V1ListTeacherRubricsResponse ToV1ListTeacherRubricsResponse(
        this ListTeacherRubricsQueryResponse queryResponse)
    {
        return new Proto.V1ListTeacherRubricsResponse
        {
            Rubrics = { queryResponse.Rubrics.ToArrayBy(ToProto) }
        };
    }

    private static RubricCriterionInput ToCreateRubricCriterionItem(Proto.RubricCriterionInput input)
    {
        return new RubricCriterionInput
        {
            Name = input.Name,
            Description = input.HasDescription ? input.Description : null,
            MaxScore = input.MaxScore,
            CommentRequired = input.CommentRequired,
            Position = input.Position
        };
    }

    private static Proto.RubricInfo ToProto(this Rubric rubric)
    {
        return new Proto.RubricInfo
        {
            Id = (long)rubric.Id,
            TeacherId = (long)rubric.TeacherId,
            Name = rubric.Name
        };
    }

    private static Proto.RubricCriterionInfo ToProto(this RubricCriterion criterion)
    {
        var info = new Proto.RubricCriterionInfo
        {
            Id = (long)criterion.Id,
            Name = criterion.Name,
            MaxScore = criterion.MaxScore,
            CommentRequired = criterion.CommentRequired,
            Position = criterion.Position
        };

        if (criterion.Description is not null)
        {
            info.Description = criterion.Description;
        }

        return info;
    }
}
