namespace ServiceableBus;

public interface IServiceableBusEventHandler<T> where T : IServiceableBusEvent
{
    public Task Handle(T @event);
}