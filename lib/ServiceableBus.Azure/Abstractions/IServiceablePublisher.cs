using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceablePublisher
{
    public Task PublishAsync<T>(T message, Func<ServiceablePropertyBag> action) where T : IServiceableBusEvent;
};