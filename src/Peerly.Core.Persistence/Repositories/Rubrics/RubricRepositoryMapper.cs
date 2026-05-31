using Peerly.Core.Identifiers;
using Peerly.Core.Models.Rubrics;
using Peerly.Core.Persistence.Repositories.RubricCriteria.Models;
using Peerly.Core.Persistence.Repositories.Rubrics.Models;

namespace Peerly.Core.Persistence.Repositories.Rubrics;

internal static class RubricRepositoryMapper
{
    public static Rubric ToRubric(this RubricDb rubricDb)
    {
        return new Rubric
        {
            Id = new RubricId(rubricDb.Id),
            TeacherId = new TeacherId(rubricDb.TeacherId),
            Name = rubricDb.Name
        };
    }

    public static RubricCriterion ToRubricCriterion(this RubricCriterionDb criterionDb)
    {
        return new RubricCriterion
        {
            Id = new RubricCriterionId(criterionDb.Id),
            RubricId = new RubricId(criterionDb.RubricId),
            Name = criterionDb.Name,
            Description = criterionDb.Description,
            MaxScore = criterionDb.MaxScore,
            CommentRequired = criterionDb.CommentRequired,
            Position = criterionDb.Position
        };
    }
}
