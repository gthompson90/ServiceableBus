using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace ServiceableBus;

public class ServiceableBusBackgroundService : IHostedService, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly IReadOnlyList<IServiceableListener> _queueListeners;

    public ServiceableBusBackgroundService(IServiceableBusOptions options, IEnumerable<IServiceableListener> queueListeners)
    {
        _client = new ServiceBusClient(options.ConnectionString);
        _queueListeners = queueListeners.ToList();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var listener in _queueListeners)
        {
            await listener.StartProcessor(_client, cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var listener in _queueListeners)
        {
            await listener.StopProcessor(cancellationToken);
            listener.Dispose();
        }
    }

        

    public async ValueTask DisposeAsync()
    {
        foreach (var listener in _queueListeners)
        {
            listener.Dispose();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}