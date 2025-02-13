using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Options;

internal class ServiceablePublisherOptions : IServiceablePublisherOptions
{
    public ServiceablePublisherOptions(string queueName, Type messageType)
    {
        QueueName = queueName;
        MessageType = messageType;
    }

    public string QueueName { get; init; }
    public Type MessageType { get; init; }
}