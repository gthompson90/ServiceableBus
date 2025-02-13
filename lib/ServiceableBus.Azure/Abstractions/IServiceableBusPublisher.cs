using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableBusPublisher
{
    public Task PublishAsync<T>(T message) where T : IServiceableBusEvent;
};