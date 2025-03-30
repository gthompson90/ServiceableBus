using ServiceableBus.Azure.Dtos;
using ServiceableBus.Contracts;

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

        return new Task<IServiceableResult>(() => new Complete());
    }
}