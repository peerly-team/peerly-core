using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Core.Messaging.Consumers.UserRegistration;
using Peerly.Core.Tools.Abstractions;

namespace Peerly.Core.Messaging.Configuration;

[ExcludeFromCodeCoverage]
internal sealed class MessagingInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddOptions<KafkaConsumerOptions>()
            .BindConfiguration(KafkaConsumerOptions.SectionName);

        services.AddScoped<UserRegistrationEventProcessor>();
        services.AddHostedService<UserRegistrationConsumer>();
    }
}
