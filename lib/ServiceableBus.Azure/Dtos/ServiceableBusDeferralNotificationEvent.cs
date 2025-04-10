namespace ServiceableBus.Azure.Dtos;

public record ServiceableBusDeferralNotificationEvent(long DeferredSequenceId, string DeferredEventTopicName, Type DeferredEventType);