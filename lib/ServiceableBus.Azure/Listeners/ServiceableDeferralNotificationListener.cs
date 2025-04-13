using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Dtos;
using ServiceableBus.Azure.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceableBus.Azure.Listeners;

internal class ServiceableDeferralNotificationListener : IServiceableDeferralNotificationListener
{
    private ServiceBusProcessor? _processor = null;
    private ServiceBusClient? _client = null;
    private readonly ServiceableBusOptions _options;

    public ServiceableDeferralNotificationListener(
        IOptions<ServiceableBusOptions> options)
    {
        _options = options.Value;
    }

    public ValueTask DisposeAsync()
    {
        _processor?.DisposeAsync();
        return ValueTask.CompletedTask;
    }

    public async Task StartProcessor(
        ServiceBusClient client, 
        CancellationToken cancellationToken)
    {
        _client = client;
        _processor = client.CreateProcessor(_options.DeferralNotificationQueueName);
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopProcessor(
        CancellationToken cancellationToken)
    {
        if (_processor == null)
            return;

        await _processor.StopProcessingAsync(cancellationToken);
        _processor?.DisposeAsync();
    }

    internal async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                IncludeFields = true,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };


            var body = Encoding.UTF8.GetString(args.Message.Body.ToArray());

            var eventInstance = JsonSerializer.Deserialize<ServiceableBusDeferralNotificationEvent>(body, options);

            ServiceBusReceiver receiver = _client.CreateReceiver(eventInstance.DeferredEventTopicName);
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveDeferredMessageAsync(eventInstance.DeferredSequenceId);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            await args.DeadLetterMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error: {args.Exception}");
        return Task.CompletedTask;
    }
}
