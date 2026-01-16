using System.Diagnostics.CodeAnalysis;

namespace ServiceableBus.Aws.Options;

[ExcludeFromCodeCoverage]
public class ServiceableBusOptions
{
    public string Region { get; init; } = string.Empty;
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
}
