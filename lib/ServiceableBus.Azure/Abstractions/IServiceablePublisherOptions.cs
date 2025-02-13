namespace ServiceableBus.Azure.Abstractions;

public interface IServiceablePublisherOptions
{
    public Type MessageType { get; init; }

    public string QueueName { get; init; }
}