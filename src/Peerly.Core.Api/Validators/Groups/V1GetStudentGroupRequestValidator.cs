using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Groups;

internal sealed class V1GetStudentGroupRequestValidator : AbstractValidator<V1GetStudentGroupRequest>
{
    public V1GetStudentGroupRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
