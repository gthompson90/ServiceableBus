namespace ServiceableBus;

public interface IServiceableQueueListener<T> : IServiceableListener where T : IServiceableBusEvent
{
}
