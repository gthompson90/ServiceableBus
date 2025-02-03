using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServiceableBus
{
    public class ServiceableBusBackgroundService : IHostedService, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly List<string> _queueOrTopicNames;
        private readonly List<ServiceBusProcessor> _processors;
        private readonly IServiceProvider _serviceProvider;

        public ServiceableBusBackgroundService(string connectionString, IServiceProvider serviceProvider)
        {
            _client = new ServiceBusClient(connectionString);
            _queueOrTopicNames = new();
            _processors = new List<ServiceBusProcessor>();
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var registeredHandlers = _serviceProvider.GetServices(typeof(IServiceableBusEventHandler<>));

            var registeredHandlerTypes = registeredHandlers.Select(h => h.GetType().GetGenericArguments().First()).ToList();

            foreach (var registeredHandlerType in registeredHandlerTypes)
            {
                var topicName = (string)registeredHandlerType.GetProperty("TopicName").GetValue(null);
                var processor = _client.CreateProcessor(topicName, new ServiceBusProcessorOptions());
                processor.ProcessMessageAsync += ProcessMessageAsync;
                processor.ProcessErrorAsync += ProcessErrorAsync;
                await processor.StartProcessingAsync(cancellationToken);
                _processors.Add(processor);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var processor in _processors)
            {
                await processor.StopProcessingAsync(cancellationToken);
                await processor.DisposeAsync();
            }
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                var eventType = args.Message.ApplicationProperties["EventType"].ToString();
                var eventTypeInstance = Type.GetType(eventType);

                if (eventTypeInstance != null)
                {
                    var eventInstance = JsonSerializer.Deserialize(messageBody, eventTypeInstance);

                    if (eventInstance != null)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var handlerType = typeof(IServiceableBusEventHandler<>).MakeGenericType(eventTypeInstance);
                            var handler = scope.ServiceProvider.GetRequiredService(handlerType);

                            var handleMethod = handlerType.GetMethod("Handle");
                            if (handleMethod != null)
                            {
                                await (Task)handleMethod.Invoke(handler, new[] { eventInstance });
                            }
                        }
                    }
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error: {args.Exception}");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var processor in _processors)
            {
                await processor.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }

        internal void AddTopic<T>(string topicName) where T : ServiceableBusEvent
        {
            _queueOrTopicNames.Add(topicName);
        }
    }
}
