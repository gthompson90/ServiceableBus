using Microsoft.Extensions.Hosting;
using ServiceableBus.Aws.Abstractions;

namespace ServiceableBus.Aws;

public class ServiceableBusBackgroundService : IHostedService, IAsyncDisposable
{
    private readonly IReadOnlyList<IServiceableListener> _listeners;

    public ServiceableBusBackgroundService(IEnumerable<IServiceableListener> listeners)
    {
        _listeners = listeners.ToList();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var listener in _listeners)
        {
            await listener.StartProcessor(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var listener in _listeners)
        {
            await listener.StopProcessor(cancellationToken);
            listener.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var listener in _listeners)
        {
            listener.Dispose();
        }
    }
}
