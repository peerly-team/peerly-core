using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Users;

internal sealed class V1SearchUsersRequestValidator : AbstractValidator<V1SearchUsersRequest>
{
    public V1SearchUsersRequestValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull();

        RuleFor(x => x.Filter.Query)
            .MinimumLength(3)
            .When(x => x.Filter != null);

        RuleFor(x => x.Limit)
            .GreaterThan(0);

        RuleForEach(x => x.Filter.Roles)
            .Must(role => role is Role.Teacher or Role.Student)
            .When(x => x.Filter != null);
    }
}
