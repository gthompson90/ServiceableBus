namespace ServiceableBus;

internal class ServiceableQueueListenerOptions<T> : IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public ServiceableQueueListenerOptions(string queueName)
    {
        QueueName = queueName;
    }

    public string QueueName { get; init; }
}