namespace ServiceableBus;

public class ServiceableBusOptions : IServiceableBusOptions
{
    public ServiceableBusOptions(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; init; }
}