using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Peerly.Core.Abstractions.UnitOfWork;
using Peerly.Core.Identifiers;
using Peerly.Core.Messaging.Consumers.UserRegistration.Models;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.Messaging.Consumers.UserRegistration;

internal sealed class UserRegistrationEventProcessor
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<UserRegistrationEventProcessor> _logger;

    public UserRegistrationEventProcessor(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        ILogger<UserRegistrationEventProcessor> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _logger = logger;
    }

    public async Task ProcessAsync(UserRegistrationEvent message, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        _ = message.Role switch
        {
            UserRole.Student => await ProcessStudentAsync(unitOfWork, message, cancellationToken),
            UserRole.Teacher => await ProcessTeacherAsync(unitOfWork, message, cancellationToken),
            UserRole.Admin => LogAdminNotSupported(message),
            _ => throw new InvalidOperationException($"Unknown role: {message.Role}")
        };

        _logger.LogInformation(
            "{Processor} | Processed user registration | UserId: {UserId}, Role: {Role}",
            nameof(UserRegistrationEventProcessor),
            message.Id,
            message.Role);
    }

    private bool LogAdminNotSupported(UserRegistrationEvent message)
    {
        _logger.LogWarning(
            "{Processor} | Admin role is not yet supported | UserId: {UserId}",
            nameof(UserRegistrationEventProcessor),
            message.Id);
        return false;
    }

    private static async Task<bool> ProcessStudentAsync(
        ICommonUnitOfWork unitOfWork,
        UserRegistrationEvent message,
        CancellationToken cancellationToken)
    {
        var addItem = new StudentAddItem
        {
            Id = new StudentId(message.Id),
            Email = message.Email,
            Name = message.Name,
            CreationTime = message.Timestamp
        };

        return await unitOfWork.StudentRepository.AddIfNotExistsAsync(addItem, cancellationToken);
    }

    private static async Task<bool> ProcessTeacherAsync(
        ICommonUnitOfWork unitOfWork,
        UserRegistrationEvent message,
        CancellationToken cancellationToken)
    {
        var addItem = new TeacherAddItem
        {
            Id = new TeacherId(message.Id),
            Email = message.Email,
            Name = message.Name,
            CreationTime = message.Timestamp
        };

        return await unitOfWork.TeacherRepository.AddIfNotExistsAsync(addItem, cancellationToken);
    }
}
