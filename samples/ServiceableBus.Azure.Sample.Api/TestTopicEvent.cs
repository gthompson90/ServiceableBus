using ServiceableBus.Azure;
using ServiceableBus.Contracts;

namespace ServiceableBus.Sample.Api;

//The event must inherit from the ServiceableBusEvent class and provide a concrete payload type.
public class TestTopicEvent : ServiceableBusEvent<TestEvent.TestEventPayload>
{
    //It is useful to define the topic/queue as a const in the event so it can be referenced when adding the Listener.
    public const string Topic = "test-topic-event";

    //Payloads must inherit from the IServiceableBusPayload interface
    public record TestEventPayload(string Field1, int Field2, int Field3) : IServiceableBusPayload;
}