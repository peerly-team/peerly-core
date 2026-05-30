using System.Linq;
using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Participants;

internal sealed class V1BulkAddGroupStudentsRequestValidator : AbstractValidator<V1BulkAddGroupStudentsRequest>
{
    public V1BulkAddGroupStudentsRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0);

        RuleFor(x => x.StudentIds)
            .NotEmpty()
            .Must(studentIds => studentIds.Distinct().Count() == studentIds.Count)
            .WithMessage("Student ids must be unique");

        RuleForEach(x => x.StudentIds)
            .GreaterThan(0);
    }
}
