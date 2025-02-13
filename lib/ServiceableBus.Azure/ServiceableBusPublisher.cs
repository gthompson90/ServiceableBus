using Azure.Messaging.ServiceBus;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceableBus.Azure;

internal class ServiceableBusPublisher : IServiceableBusPublisher
{
    private readonly IServiceableBusClientFactory _clientFactory;
    private readonly IEnumerable<IServiceablePublisherOptions> _options;

    public ServiceableBusPublisher(IServiceableBusClientFactory clientFactory, IEnumerable<IServiceablePublisherOptions> options)
    {
        _clientFactory = clientFactory;
        _options = options;
    }

    public async Task PublishAsync<T>(T message) where T : IServiceableBusEvent
    {
        var sender = _clientFactory.CreateSender(_options.First(x => x.MessageType == typeof(T)));
        if (sender is null)
        {
            throw new InvalidOperationException("Sender is null");
        }

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

        await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(eventInstance ?? throw new ArgumentNullException(nameof(message)))));
    }
}
