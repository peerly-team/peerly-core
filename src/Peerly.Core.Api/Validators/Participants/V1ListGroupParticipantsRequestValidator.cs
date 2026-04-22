using FluentValidation;
using Peerly.Core.V1;

namespace Peerly.Core.Api.Validators.Participants;

internal sealed class V1ListGroupParticipantsRequestValidator : AbstractValidator<V1ListGroupParticipantsRequest>
{
    public V1ListGroupParticipantsRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0);
    }
}
