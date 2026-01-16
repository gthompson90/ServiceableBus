using ServiceableBus.Aws.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Aws.Options;

[ExcludeFromCodeCoverage]
internal class ServiceablePublisherOptions : IServiceablePublisherOptions
{
    public ServiceablePublisherOptions(string queueOrTopicName, Type messageType)
    {
        QueueOrTopicName = queueOrTopicName;
        MessageType = messageType;
    }

    public string QueueOrTopicName { get; init; }
    public Type MessageType { get; init; }
}
