﻿namespace ServiceableBus.Sample.Api;

public class TestEvent : ServiceableBusEvent<TestEvent.TestEventPayload>
{
    public const string Topic = "test-event";

    public record TestEventPayload(string Field1, int Field2, int Field3) : IServiceableBusPayload;
}