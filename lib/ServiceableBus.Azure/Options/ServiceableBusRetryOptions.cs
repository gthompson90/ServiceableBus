using ServiceableBus.Azure.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Azure.Options;

[ExcludeFromCodeCoverage]
internal class ServiceableRetryOptions : IServiceableRetryOptions
{
    public ServiceableRetryOptions(int retries, int delay)
    {
        MaxRetries = retries;
        DelaySeconds = delay;
    }

    public int MaxRetries { get; init; }
    public int DelaySeconds { get; init; }
}