using ServiceableBus.Aws;
using ServiceableBus.Contracts;

namespace ServiceableBus.Sample.Api;

//The event must inherit from the ServiceableBusEvent class and provide a concrete payload type.
public class TestTopicEvent : ServiceableBusEvent<TestEvent.TestEventPayload>
{
    //For SNS topics, we use the TopicArn for publishing
    public const string TopicArn = "arn:aws:sns:us-east-1:123456789012:test-topic-event";
    
    //For subscribing, we need the SQS queue URL that is subscribed to the topic
    public const string QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/test-topic-event-queue";

    //Payloads must inherit from the IServiceableBusPayload interface
    public record TestEventPayload(string Field1, int Field2, int Field3) : IServiceableBusPayload;
}
