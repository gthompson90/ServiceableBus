using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Azure.Options;

[ExcludeFromCodeCoverage]
internal class ServiceableQueueListenerOptions<T> : IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public ServiceableQueueListenerOptions(string queueName)
    {
        QueueName = queueName;
    }

    public string QueueName { get; init; }
}