using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Extensions;
using ServiceableBus.Azure.Options;
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
        QueueName = TestEvent.Queue 
    });

//builder.AddServiceableBusTopicListener(() =>
//    new ServiceableTopicListenerOptions<TestTopicEvent>()
//    { 
//        SubscriptionName =  appTopicSubscriptionName,
//        TopicName = TestTopicEvent.Topic
//    });

builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
builder.RegisterServiceableBusHandler<TestTopicEvent, TestTopicEventServiceBusHandler>();

//Add your Event Senders here BEFORE adding the ServiceableBus.
builder.AddServiceableBusEventSender<TestEvent>(TestEvent.Queue);
builder.AddServiceableBusEventSender<TestTopicEvent>(TestTopicEvent.Topic);

builder.AddServiceableBus();

var app = builder.Build();

var sender = app.Services.GetService<IServiceablePublisher>();

await sender.PublishAsync(new TestEvent 
{ 
    MessageTypeName = "TestEvent",
    Payload = new TestEvent.TestEventPayload("Test", 1, 4) 
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();