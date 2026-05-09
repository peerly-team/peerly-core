using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1CreateSubmittedHomeworkRequestValidator : AbstractValidator<V1CreateSubmittedHomeworkRequest>
{
    public V1CreateSubmittedHomeworkRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.Comment)
            .NotEmpty();
    }
}
