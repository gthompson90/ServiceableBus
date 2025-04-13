using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;

internal interface IServiceableDeferralNotificationPublisher
{
    Task PublishDeferralNotification(long sequenceNumber, int deferralInMinutes, string deferredEventTopicName, Type deferredEventType);
}