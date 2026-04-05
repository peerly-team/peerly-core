namespace Peerly.Core.Messaging.Configuration;

internal sealed class KafkaConsumerOptions
{
    public const string SectionName = "KafkaConsumer";

    public string BootstrapServers { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public string Topic { get; set; } = null!;
    public bool EnableAutoCommit { get; set; }
    public string AutoOffsetReset { get; set; } = "earliest";
}
