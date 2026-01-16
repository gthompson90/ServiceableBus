using ServiceableBus.Contracts;

namespace ServiceableBus.Aws.Abstractions;

public interface IServiceableQueueListener<T> : IServiceableListener where T : IServiceableBusEvent
{
}
