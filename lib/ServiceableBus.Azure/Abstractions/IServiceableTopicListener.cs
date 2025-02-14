using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableTopicListener<T> : IServiceableListener where T : IServiceableBusEvent
{
}