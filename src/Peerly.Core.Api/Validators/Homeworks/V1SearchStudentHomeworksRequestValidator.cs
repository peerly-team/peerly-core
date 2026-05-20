using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1SearchStudentHomeworksRequestValidator : AbstractValidator<V1SearchStudentHomeworksRequest>
{
    public V1SearchStudentHomeworksRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.PaginationInfo.Offset)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaginationInfo.PageSize)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Filter.HomeworkStatuses)
            .Must(homeworkStatuses => !homeworkStatuses.Contains(HomeworkStatus.Draft))
            .WithMessage("'Homework Statuses' must not contain 'Draft'.");
    }
}
