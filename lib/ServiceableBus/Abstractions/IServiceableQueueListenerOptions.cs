namespace ServiceableBus.Azure.Abstractions;

internal interface IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public string QueueName { get; init; }
}