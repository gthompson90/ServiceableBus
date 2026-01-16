using ServiceableBus.Aws;
using ServiceableBus.Contracts;

namespace ServiceableBus.Sample.Api;

//The event must inherit from the ServiceableBusEvent class and provide a concrete payload type.
public class TestEvent : ServiceableBusEvent<TestEvent.TestEventPayload>
{
    //It is useful to define the queue URL as a const in the event so it can be referenced when adding the Listener.
    public const string QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/test-event";

    //Payloads must inherit from the IServiceableBusPayload interface
    public record TestEventPayload(string Field1, int Field2, int Field3) : IServiceableBusPayload;
}
