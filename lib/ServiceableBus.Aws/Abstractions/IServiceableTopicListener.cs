using ServiceableBus.Contracts;

namespace ServiceableBus.Aws.Abstractions;

public interface IServiceableTopicListener<T> : IServiceableListener where T : IServiceableBusEvent
{
}
