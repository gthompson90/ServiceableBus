using ServiceableBus.Contracts;

namespace ServiceableBus.Sample.Api;

public class TestTopicEventServiceBusHandler : IServiceableBusEventHandler<TestTopicEvent>
{
    public Task Handle(TestTopicEvent @event)
    {
        Console.WriteLine($"Handling event {@event.MessageTypeName} with payload {@event.Payload}");

        return Task.CompletedTask;
    }
}