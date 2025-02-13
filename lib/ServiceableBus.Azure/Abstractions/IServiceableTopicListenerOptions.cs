using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableTopicListenerOptions<T> where T : IServiceableBusEvent
{
    public string TopicName { get; init; }

    public string SubscriptionName { get; init; }
}