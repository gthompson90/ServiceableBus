using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceablePublisher
{
    public Task PublishAsync<T>(T message) where T : IServiceableBusEvent;
};