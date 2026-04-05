using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1PostponeHomeworkDeadlinesRequestValidator : AbstractValidator<V1PostponeHomeworkDeadlinesRequest>
{
    public V1PostponeHomeworkDeadlinesRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
