using ServiceableBus.Contracts;
using ServiceableBus.Contracts.Results;

namespace ServiceableBus.Sample.Api;

public class TestQueueEventServiceBusHandler : IServiceableBusEventHandler<TestQueueEvent>
{
    public Task<IServiceableResult> Handle(TestQueueEvent @event, ServiceablePropertyBag properties)
    {
        Console.WriteLine($"Handling event {@event.MessageTypeName} with payload {@event.Payload}");

        foreach (var property in properties.Properties)
        {
            Console.WriteLine($"Property: {property.Key} = {property.Value}");
        }

        return Task.FromResult<IServiceableResult>(ServiceableResult.Defer(1));
    }
}