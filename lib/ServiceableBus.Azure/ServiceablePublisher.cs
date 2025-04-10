using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Options;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceableBus.Azure;

internal class ServiceablePublisher : IServiceablePublisher
{
    private const string DeferralMessageTypename = "ServiceableBusDeferralNotificationEvent";

    private readonly IServiceableBusClientFactory _clientFactory;
    private readonly IEnumerable<IServiceablePublisherOptions> _publisherOptions;
    private readonly ServiceableBusOptions _serviceableBusOptions;

    public ServiceablePublisher(
        IServiceableBusClientFactory clientFactory, 
        IEnumerable<IServiceablePublisherOptions> publisherOptions,
        IOptions<ServiceableBusOptions> serviceableBusOptions)
    {
        _clientFactory = clientFactory;
        _publisherOptions = publisherOptions;
        _serviceableBusOptions = serviceableBusOptions.Value;
    }

    public async Task PublishAsync<T>(T message, Func<ServiceablePropertyBag>? action = null) where T : IServiceableBusEvent
    {
        var messageOptions = _publisherOptions.FirstOrDefault(x => x.MessageType == typeof(T));

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

        var sbMessage = new ServiceBusMessage()
        {
            Body = new BinaryData(Encoding.UTF8.GetBytes(eventInstance!)),
            ContentType = "application/json",
            CorrelationId = Guid.NewGuid().ToString(),
        };

        if (action is not null) { 
            foreach (var (key, value) in action.Invoke().ToDictionary())
            {
                sbMessage.ApplicationProperties.Add(key, value);
            }
        }

        await sender.SendMessageAsync(sbMessage);
    }

    public async Task PublishDeferralNotification(int sequenceNumber)
    {
        if (_serviceableBusOptions is null || string.IsNullOrWhiteSpace(_serviceableBusOptions.DeferralNotificationQueueName))
            throw new InvalidOperationException($"No publisher options found for Deferral Notifications.");

        var sender = _clientFactory.CreateDeferralNotificationSender(_serviceableBusOptions);

        var notificationMessage = new ServiceBusMessage()
        {
            Body = new BinaryData(Encoding.UTF8.GetBytes(sequenceNumber.ToString())),
            ContentType = "application/json",
            CorrelationId = Guid.NewGuid().ToString(),
        };

        await sender.SendMessageAsync(notificationMessage);
    }
}