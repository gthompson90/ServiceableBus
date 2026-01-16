using ServiceableBus.Contracts;

namespace ServiceableBus.Aws.Abstractions;

public interface IServiceableTopicListenerOptions<T> where T : IServiceableBusEvent
{
    public string QueueUrl { get; init; }
}
