using ServiceableBus.Azure.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Azure.Options;

[ExcludeFromCodeCoverage]
internal class ServiceablePublisherOptions : IServiceablePublisherOptions
{
    public ServiceablePublisherOptions(string queueName, Type messageType)
    {
        QueueOrTopicName = queueName;
        MessageType = messageType;
    }

    public string QueueOrTopicName { get; init; }
    public Type MessageType { get; init; }
}