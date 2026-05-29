using System;
using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Features.V1.Rubrics.Shared.Models;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.UpdateRubric;

internal static class UpdateRubricHandlerMapper
{
    public static IReadOnlyCollection<RubricCriterionAddItem> ToRubricCriterionAddItems(
        this IReadOnlyCollection<RubricCriterionInput> criteria,
        RubricId rubricId,
        DateTimeOffset creationTime)
    {
        return criteria.ToArrayBy(
            c => new RubricCriterionAddItem
            {
                RubricId = rubricId,
                Name = c.Name,
                Description = c.Description,
                MaxScore = c.MaxScore,
                CommentRequired = c.CommentRequired,
                Position = c.Position,
                CreationTime = creationTime
            });
    }
}
