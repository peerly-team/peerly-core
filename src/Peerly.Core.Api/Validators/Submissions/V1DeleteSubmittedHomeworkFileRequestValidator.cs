using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1DeleteSubmittedHomeworkFileRequestValidator : AbstractValidator<V1DeleteSubmittedHomeworkFileRequest>
{
    public V1DeleteSubmittedHomeworkFileRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.FileId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
