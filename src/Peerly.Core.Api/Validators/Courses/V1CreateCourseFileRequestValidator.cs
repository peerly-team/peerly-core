using System;
using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Courses;

internal sealed class V1CreateCourseFileRequestValidator : AbstractValidator<V1CreateCourseFileRequest>
{
    public V1CreateCourseFileRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.StorageId)
            .NotEmpty()
            .Must(storageId => Guid.TryParse(storageId, out _));

        RuleFor(x => x.FileName)
            .NotEmpty();

        RuleFor(x => x.FileSize)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);
    }
}
