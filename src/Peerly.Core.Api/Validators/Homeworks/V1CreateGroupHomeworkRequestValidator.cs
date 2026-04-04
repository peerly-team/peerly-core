using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1CreateGroupHomeworkRequestValidator : AbstractValidator<V1CreateGroupHomeworkRequest>
{
    public V1CreateGroupHomeworkRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.AmountOfReviewers)
            .GreaterThan(0);

        RuleFor(x => x.DiscrepancyThreshold)
            .InclusiveBetween(0, 100);
    }
}
