using System;
using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Submissions;

internal sealed class V1CreateSubmittedHomeworkFileRequestValidator : AbstractValidator<V1CreateSubmittedHomeworkFileRequest>
{
    public V1CreateSubmittedHomeworkFileRequestValidator()
    {
        RuleFor(x => x.SubmittedHomeworkId)
            .GreaterThan(0);

        RuleFor(x => x.StorageId)
            .NotEmpty()
            .Must(storageId => Guid.TryParse(storageId, out _));

        RuleFor(x => x.FileName)
            .NotEmpty();

        RuleFor(x => x.FileSize)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
