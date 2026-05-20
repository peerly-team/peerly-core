using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Homeworks;

internal sealed class V1SearchTeacherHomeworksRequestValidator : AbstractValidator<V1SearchTeacherHomeworksRequest>
{
    public V1SearchTeacherHomeworksRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.PaginationInfo.Offset)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PaginationInfo.PageSize)
            .GreaterThanOrEqualTo(0);
    }
}
