using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Azure.Options;

[ExcludeFromCodeCoverage]
public class ServiceableTopicListenerOptions<T> : IServiceableTopicListenerOptions<T> where T : IServiceableBusEvent
{
    public required string TopicName { get; init; }

    public required string SubscriptionName { get; init; }
}