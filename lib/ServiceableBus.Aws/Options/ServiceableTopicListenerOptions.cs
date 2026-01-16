using ServiceableBus.Aws.Abstractions;
using ServiceableBus.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Aws.Options;

[ExcludeFromCodeCoverage]
public class ServiceableTopicListenerOptions<T> : IServiceableTopicListenerOptions<T> where T : IServiceableBusEvent
{
    public required string QueueUrl { get; init; }
}
