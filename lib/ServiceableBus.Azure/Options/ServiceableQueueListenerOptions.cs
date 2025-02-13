using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Azure.Options;

[ExcludeFromCodeCoverage]
public class ServiceableQueueListenerOptions<T> : IServiceableQueueListenerOptions<T> where T : IServiceableBusEvent
{
    public required string QueueName { get; init; }
}