using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Options;

internal class ServiceableQueueListenerOptions<T> : IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public ServiceableQueueListenerOptions(string queueName)
    {
        QueueName = queueName;
    }

    public string QueueName { get; init; }
}