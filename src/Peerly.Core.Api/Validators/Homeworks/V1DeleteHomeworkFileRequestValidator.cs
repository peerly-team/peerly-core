using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1DeleteHomeworkFileRequestValidator : AbstractValidator<V1DeleteHomeworkFileRequest>
{
    public V1DeleteHomeworkFileRequestValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.FileId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
