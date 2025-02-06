namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableBusOptions
{
    public string ConnectionString { get; init; }
}