namespace ServiceableBus.Contracts;

public interface IServiceableBusEventHandler<T> where T : IServiceableBusEvent
{
    public Task Handle(T @event);
}