namespace ServiceableBus;

public abstract class ServiceableBusEvent
{
    public required string TopicName { get; init; }
    public required string MessageTypeName { get; init; }
    public DateTime CreatedAt { get; init; }
    public required IServiceableBusPayload Payload { get; init; }
}
