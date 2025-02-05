using Azure.Messaging.ServiceBus;

namespace ServiceableBus;

public interface IServiceableListener : IDisposable
{
    public Type EventType { get; }
    public Task StartProcessor(ServiceBusClient client, CancellationToken cancellationToken);
    public Task StopProcessor(CancellationToken cancellationToken);
}
