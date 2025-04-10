using Azure.Messaging.ServiceBus;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableListener : IAsyncDisposable
{
    public Type EventType { get; }
    public Task StartProcessor(ServiceBusClient client, CancellationToken cancellationToken);
    public Task StopProcessor(CancellationToken cancellationToken);
}