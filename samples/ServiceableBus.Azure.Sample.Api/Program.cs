using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Extensions;
using ServiceableBus.Sample.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

//Adding options is required for the ServiceableBus configuration.
builder.Services.AddOptions();

//Add your Listeners and Handlers here BEFORE adding the ServiceableBus.
builder.AddServiceableBusTopicListener<TestEvent>(TestEvent.Topic);
builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
builder.AddServiceableBusEventSender<TestEvent>(TestEvent.Topic);

builder.AddServiceableBus();

var app = builder.Build();

var sender = app.Services.GetService<IServiceableBusPublisher>();

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