using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Dtos;
using ServiceableBus.Azure.Options;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceableBus.Azure;

internal class ServiceableDeferralNotificationPublisher : IServiceableDeferralNotificationPublisher
{
    private readonly IServiceableBusClientFactory _clientFactory;
    private readonly ServiceableBusOptions _serviceableBusOptions;

    public ServiceableDeferralNotificationPublisher(
        IServiceableBusClientFactory clientFactory, 
        IOptions<ServiceableBusOptions> serviceableBusOptions)
    {
        _clientFactory = clientFactory;
        _serviceableBusOptions = serviceableBusOptions.Value;
    }

    public async Task PublishDeferralNotification(long sequenceNumber, int deferralInMinutes, string deferredEventTopicName, Type deferredEventType)
    {
        if (_serviceableBusOptions is null || string.IsNullOrWhiteSpace(_serviceableBusOptions.DeferralNotificationQueueName))
            throw new InvalidOperationException($"No publisher options found for Deferral Notifications.");

        var sender = _clientFactory.CreateDeferralNotificationSender(_serviceableBusOptions);

        var notificationEvent = new ServiceableBusDeferralNotificationEvent(sequenceNumber, deferredEventTopicName, deferredEventType);

        var options = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            IncludeFields = true,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        var eventInstance = JsonSerializer.Serialize(notificationEvent, options);

        var notificationMessage = new ServiceBusMessage()
        {
            Body = new BinaryData(Encoding.UTF8.GetBytes(eventInstance)),
            ContentType = "application/json",
            CorrelationId = Guid.NewGuid().ToString(),
            ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddMinutes(5),
        };

        await sender.SendMessageAsync(notificationMessage);
    }
}