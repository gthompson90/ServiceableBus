namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableQueueListener<T> : IServiceableListener where T : IServiceableBusEvent
{
}