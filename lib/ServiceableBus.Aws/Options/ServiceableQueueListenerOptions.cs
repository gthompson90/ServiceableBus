using ServiceableBus.Aws.Abstractions;
using ServiceableBus.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Aws.Options;

[ExcludeFromCodeCoverage]
public class ServiceableQueueListenerOptions<T> : IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public required string QueueUrl { get; init; }
}
