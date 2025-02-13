using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Azure.Options;

[ExcludeFromCodeCoverage]
public class ServiceableBusOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}