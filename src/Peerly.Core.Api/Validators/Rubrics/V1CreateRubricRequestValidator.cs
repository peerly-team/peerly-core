using System.Linq;
using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Rubrics;

internal sealed class V1CreateRubricRequestValidator : AbstractValidator<V1CreateRubricRequest>
{
    public V1CreateRubricRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Criteria)
            .NotEmpty()
            .Must(criteria => criteria.Select(c => c.Position).ToHashSet().Count == criteria.Count)
            .WithMessage("Criteria positions must be unique.");

        RuleForEach(x => x.Criteria)
            .ChildRules(criterion =>
            {
                criterion.RuleFor(c => c.Name).NotEmpty();
                criterion.RuleFor(c => c.MaxScore).InclusiveBetween(1, 100);
                criterion.RuleFor(c => c.Position).GreaterThanOrEqualTo(0);
            });
    }
}
