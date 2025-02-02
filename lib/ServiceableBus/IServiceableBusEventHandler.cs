namespace ServiceableBus
{
    public interface IServiceableBusEventHandler<T> where T : ServiceableBusEvent
    {
        public void Handle(T @event);
    }
}
