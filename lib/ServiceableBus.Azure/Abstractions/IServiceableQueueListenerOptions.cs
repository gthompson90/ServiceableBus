using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public string QueueName { get; init; }
}