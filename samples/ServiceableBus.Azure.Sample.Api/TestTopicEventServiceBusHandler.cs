using ServiceableBus.Contracts;
using ServiceableBus.Contracts.Results;

namespace ServiceableBus.Sample.Api;

public class TestTopicEventServiceBusHandler : IServiceableBusEventHandler<TestTopicEvent>
{
    public Task<IServiceableResult> Handle(TestTopicEvent @event, ServiceablePropertyBag properties)
    {
        Console.WriteLine($"Handling event {@event.MessageTypeName} with payload {@event.Payload}");

        foreach (var property in properties.Properties)
        {
            Console.WriteLine($"Property: {property.Key} = {property.Value}");
        }

        return Task.FromResult<IServiceableResult>(ServiceableResult.Complete());
    }
}