using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peerly.Core.Messaging.Configuration;

namespace Peerly.Core.Messaging.Consumers.UserRegistration;

internal sealed class UserRegistrationConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaConsumerOptions _options;
    private readonly ILogger<UserRegistrationConsumer> _logger;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserRegistrationConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaConsumerOptions> options,
        ILogger<UserRegistrationConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_options.AutoOffsetReset, ignoreCase: true),
            EnableAutoCommit = _options.EnableAutoCommit
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_options.Topic);

        _logger.LogInformation(
            "{Consumer} | Started consuming from topic {Topic}",
            nameof(UserRegistrationConsumer),
            _options.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ConsumeNextMessage(consumer, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ConsumeNextMessage(IConsumer<string, string> consumer, CancellationToken stoppingToken)
    {
        var consumeResult = new ConsumeResult<string, string>();

        try
        {
            consumeResult = consumer.Consume(stoppingToken);

            if (consumeResult?.Message is null)
            {
                return;
            }

            var message = JsonSerializer.Deserialize<UserRegistrationEvent>(consumeResult.Message.Value, s_jsonOptions);

            if (message is null)
            {
                _logger.LogWarning(
                    "{Consumer} | Failed to deserialize message | Offset: {Offset}",
                    nameof(UserRegistrationConsumer),
                    consumeResult.Offset);
                consumer.Commit(consumeResult);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<UserRegistrationEventProcessor>();
            await processor.ProcessAsync(message, stoppingToken);

            consumer.Commit(consumeResult);
        }
        catch (JsonException ex)
        {
            LogError(ex, consumeResult?.Offset);

            if (consumeResult is not null)
            {
                consumer.Commit(consumeResult);
            }
        }
        catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
        {
            _logger.LogWarning(
                "{Consumer} | Topic not yet available, retrying...",
                nameof(UserRegistrationConsumer));
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogError(ex, consumeResult?.Offset);
        }
    }

    private void LogError(Exception ex, long? offset)
    {
        _logger.LogError(
            ex,
            "{Consumer} | Error processing message | Offset: {Offset}, Error: {ErrorMessage}",
            nameof(UserRegistrationConsumer),
            offset,
            ex.Message);
    }
}
