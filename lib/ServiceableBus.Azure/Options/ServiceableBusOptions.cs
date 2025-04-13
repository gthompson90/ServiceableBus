using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Azure.Options;

[ExcludeFromCodeCoverage]
public class ServiceableBusOptions
{
    public string ConnectionString { get; init; } = string.Empty;

    public string DeferralNotificationQueueName { get; init; } = string.Empty;
}