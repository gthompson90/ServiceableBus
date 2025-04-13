using Azure.Messaging.ServiceBus;
using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableDeferralNotificationListener : IAsyncDisposable
{
    public Task StartProcessor(ServiceBusClient client, CancellationToken cancellationToken);
    public Task StopProcessor(CancellationToken cancellationToken);
}