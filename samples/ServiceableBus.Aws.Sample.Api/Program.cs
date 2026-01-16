using ServiceableBus.Aws.Abstractions;
using ServiceableBus.Aws.Extensions;
using ServiceableBus.Aws.Options;
using ServiceableBus.Contracts;
using ServiceableBus.Sample.Api;

//string appTopicSubscriptionName = "TestEventSubscription";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

//Adding options is required for the ServiceableBus configuration.
builder.Services.AddOptions();

//Add your Listeners and Handlers here BEFORE adding the ServiceableBus.
builder.AddServiceableBusQueueListener(() =>
    new ServiceableQueueListenerOptions<TestEvent>()
    {
        QueueUrl = TestEvent.QueueUrl
    });

//builder.AddServiceableBusTopicListener(() =>
//    new ServiceableTopicListenerOptions<TestTopicEvent>()
//    { 
//        QueueUrl = TestTopicEvent.QueueUrl
//    });

builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
builder.RegisterServiceableBusHandler<TestTopicEvent, TestTopicEventServiceBusHandler>();

//Add your Event Senders here BEFORE adding the ServiceableBus.
builder.AddServiceableBusEventSender<TestEvent>(TestEvent.QueueUrl);
builder.AddServiceableBusEventSender<TestTopicEvent>(TestTopicEvent.TopicArn);

builder.AddServiceableBus();

var app = builder.Build();

var sender = app.Services.GetService<IServiceablePublisher>();

//await sender.PublishAsync(new TestEvent 
//{ 
//    MessageTypeName = "TestEvent",
//    Payload = new TestEvent.TestEventPayload("Test", 1, 4),
//    CreatedAt = DateTime.UtcNow
//}, 
//() => new ServiceablePropertyBag 
//{ 
//    Properties = [("AppName", "TestApp"), ("AnotherProperty", "WhateverValueYouLike")] 
//});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
