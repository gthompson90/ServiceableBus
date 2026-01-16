using ServiceableBus.Contracts;

namespace ServiceableBus.Aws.Abstractions;

public interface IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public string QueueUrl { get; init; }
}
