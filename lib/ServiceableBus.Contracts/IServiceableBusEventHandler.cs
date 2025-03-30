namespace ServiceableBus.Contracts;

public interface IServiceableBusEventHandler<T> where T : IServiceableBusEvent
{
    public Task<IServiceableResult> Handle(T @event, ServiceablePropertyBag properties);
}