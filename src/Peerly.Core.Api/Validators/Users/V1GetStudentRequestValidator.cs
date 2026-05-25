using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Users;

internal sealed class V1GetStudentRequestValidator : AbstractValidator<V1GetStudentRequest>
{
    public V1GetStudentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0);
    }
}
