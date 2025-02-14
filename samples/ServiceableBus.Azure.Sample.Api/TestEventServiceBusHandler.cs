using ServiceableBus.Contracts;

namespace ServiceableBus.Sample.Api;

public class TestEventServiceBusHandler : IServiceableBusEventHandler<TestEvent>
{
    public Task Handle(TestEvent @event, ServiceablePropertyBag properties)
    {
        Console.WriteLine($"Handling event {@event.MessageTypeName} with payload {@event.Payload}");

        foreach (var property in properties._properties)
        {
            Console.WriteLine($"Property: {property.Item1} = {property.Item2}");
        }

        return Task.CompletedTask;
    }
}