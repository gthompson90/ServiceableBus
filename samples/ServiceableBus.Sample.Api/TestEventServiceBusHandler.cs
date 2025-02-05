namespace ServiceableBus.Sample.Api;

public class TestEventServiceBusHandler : IServiceableBusEventHandler<TestEvent>
{
    public void Handle(TestEvent @event)
    {
        Console.WriteLine($"Handling event {@event.MessageTypeName} with payload {@event.Payload}");
    }
}