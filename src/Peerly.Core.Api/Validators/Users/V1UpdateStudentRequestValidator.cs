using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Users;

internal sealed class V1UpdateStudentRequestValidator : AbstractValidator<V1UpdateStudentRequest>
{
    public V1UpdateStudentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty();
    }
}
