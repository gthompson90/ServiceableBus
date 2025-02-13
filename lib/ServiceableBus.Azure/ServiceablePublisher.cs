using Azure.Messaging.ServiceBus;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceableBus.Azure;

internal class ServiceablePublisher : IServiceablePublisher
{
    private readonly IServiceableBusClientFactory _clientFactory;
    private readonly IEnumerable<IServiceablePublisherOptions> _options;

    public ServiceablePublisher(IServiceableBusClientFactory clientFactory, IEnumerable<IServiceablePublisherOptions> options)
    {
        _clientFactory = clientFactory;
        _options = options;
    }

    public async Task PublishAsync<T>(T message) where T : IServiceableBusEvent
    {
        var messageOptions = _options.FirstOrDefault(x => x.MessageType == typeof(T));

        if (messageOptions is null)
            throw new InvalidOperationException($"No publisher options found for message type {typeof(T).Name}.");

        var sender = _clientFactory.CreateSender(messageOptions);

        if (sender is null)
            throw new InvalidOperationException("Sender has not been initialised.");

        var options = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            IncludeFields = true,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        var eventInstance = JsonSerializer.Serialize(message, options);

        await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(eventInstance!)));
    }
}
