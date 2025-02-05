namespace ServiceableBus;

public interface IServiceableBusEventHandler<T> where T : IServiceableBusEvent
{
    public void Handle(T @event);
}
