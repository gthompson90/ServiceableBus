using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Options;
using System.Threading;

namespace ServiceableBus.Azure;

public class ServiceableBusBackgroundService : IHostedService, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceableBusOptions _options;
    private readonly ServiceBusClient _client;
    private readonly IReadOnlyList<IServiceableListener> _queueListeners;
    private IServiceableDeferralNotificationListener? _deferralNotificationListener;

    public ServiceableBusBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<ServiceableBusOptions> options, 
        IEnumerable<IServiceableListener> queueListeners)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _client = new ServiceBusClient(options.Value.ConnectionString);
        _queueListeners = queueListeners.ToList();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var listener in _queueListeners)
        {
            await listener.StartProcessor(_client, cancellationToken);
        }

        _deferralNotificationListener = _serviceProvider.GetService<IServiceableDeferralNotificationListener>();

        if (string.IsNullOrWhiteSpace(_options.DeferralNotificationQueueName) is false
            && (_deferralNotificationListener is not null))
        {
            await _deferralNotificationListener.StartProcessor(_client, cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var listener in _queueListeners)
        {
            await listener.StopProcessor(cancellationToken);
            await listener.DisposeAsync();
        } 
        
        if (_deferralNotificationListener is not null)
        {
            await _deferralNotificationListener.StopProcessor(cancellationToken);
            await _deferralNotificationListener.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var listener in _queueListeners)
        {
            await listener.DisposeAsync();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }

        if (_deferralNotificationListener is not null)
        {
            await _deferralNotificationListener.DisposeAsync();
        }
    }
}