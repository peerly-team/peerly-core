using System;
using Peerly.Core.Messaging.Consumers.UserRegistration.Models;

namespace Peerly.Core.Messaging.Consumers.UserRegistration;

internal sealed record UserRegistrationEvent
{
    public required long Id { get; init; }
    public required UserRole Role { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
