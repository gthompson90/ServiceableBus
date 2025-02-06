using ServiceableBus.Azure.Abstractions;

namespace ServiceableBus.Azure.Options;

public class ServiceableBusOptions : IServiceableBusOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}