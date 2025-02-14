using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceableBus.Azure.Listeners;

internal class ServiceableTopicListener<T> : IServiceableTopicListener<T> where T : IServiceableBusEvent
{
    private readonly IServiceProvider _serviceProvider;
    private ServiceBusProcessor? _processor = null;
    private readonly IServiceableTopicListenerOptions<T> _options;

    public ServiceableTopicListener(IServiceProvider serviceProvider, IServiceableTopicListenerOptions<T> options)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public Type EventType { get => typeof(T); }

    public void Dispose()
    {
        _processor?.DisposeAsync();
    }

    public async Task StartProcessor(ServiceBusClient client, CancellationToken cancellationToken)
    {
        _processor = client.CreateProcessor(_options.TopicName, _options.SubscriptionName);
        _processor.ProcessMessageAsync += ProcessMessageAsync<T>;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopProcessor(CancellationToken cancellationToken)
    {
        if (_processor == null)
            return;

        await _processor.StopProcessingAsync(cancellationToken);
        _processor?.DisposeAsync();
    }

    internal async Task ProcessMessageAsync<Y>(ProcessMessageEventArgs args)
    {
        try
        {
            var eventTypeInstance = typeof(Y);

            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                IncludeFields = true,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var body = Encoding.UTF8.GetString(args.Message.Body.ToArray());

            if (eventTypeInstance != null)
            {
                var eventInstance = JsonSerializer.Deserialize<Y>(body, options);

                if (eventInstance != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var handlerType = typeof(IServiceableBusEventHandler<>).MakeGenericType(eventTypeInstance);
                        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

                        var properties = new ServiceablePropertyBag { Properties = args.Message.ApplicationProperties.Select(x => (x.Key, x.Value)).ToArray() };

                        var handleMethod = handlerType.GetMethod("Handle");
                        if (handleMethod != null)
                        {
                            await (Task)handleMethod.Invoke(handler, [eventInstance, properties])!;
                        }
                    }
                }
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            await args.DeadLetterMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error: {args.Exception}");
        return Task.CompletedTask;
    }
}