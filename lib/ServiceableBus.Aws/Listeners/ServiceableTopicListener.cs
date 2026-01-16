using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using ServiceableBus.Aws.Abstractions;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceableBus.Aws.Listeners;

internal class ServiceableTopicListener<T> : IServiceableTopicListener<T> where T : IServiceableBusEvent
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceableTopicListenerOptions<T> _options;
    private readonly IServiceableBusClientFactory _clientFactory;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollingTask;

    public ServiceableTopicListener(
        IServiceProvider serviceProvider, 
        IServiceableTopicListenerOptions<T> options,
        IServiceableBusClientFactory clientFactory)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _clientFactory = clientFactory;
    }

    public Type EventType { get => typeof(T); }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    public Task StartProcessor(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _pollingTask = Task.Run(() => PollMessagesAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public async Task StopProcessor(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        
        if (_pollingTask != null)
        {
            try
            {
                await _pollingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }
    }

    private async Task PollMessagesAsync(CancellationToken cancellationToken)
    {
        var sqsClient = _clientFactory.GetSqsClient();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _options.QueueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20,
                    MessageAttributeNames = new List<string> { "All" }
                };

                var response = await sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);

                foreach (var message in response.Messages)
                {
                    await ProcessMessageAsync(sqsClient, message, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error polling messages: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    internal async Task ProcessMessageAsync(IAmazonSQS sqsClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var eventTypeInstance = typeof(T);

            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                IncludeFields = true,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            // SNS wraps the message, so we need to extract it
            string body;
            try
            {
                var snsMessage = JsonSerializer.Deserialize<SnsMessageWrapper>(message.Body);
                body = snsMessage?.Message ?? message.Body;
            }
            catch
            {
                // If it's not an SNS message, use the body directly
                body = message.Body;
            }

            if (eventTypeInstance != null)
            {
                var eventInstance = JsonSerializer.Deserialize<T>(body, options);

                if (eventInstance != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var handlerType = typeof(IServiceableBusEventHandler<>).MakeGenericType(eventTypeInstance);
                        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

                        var properties = new ServiceablePropertyBag 
                        { 
                            Properties = message.MessageAttributes
                                .Select(x => (x.Key, (object)(x.Value.StringValue ?? string.Empty)))
                                .ToArray() 
                        };

                        var handleMethod = handlerType.GetMethod("Handle");
                        if (handleMethod != null)
                        {
                            await (Task)handleMethod.Invoke(handler, [eventInstance, properties])!;
                        }
                    }
                }
            }

            // Delete the message after successful processing
            await sqsClient.DeleteMessageAsync(_options.QueueUrl, message.ReceiptHandle, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            // Message will remain in queue and be retried or moved to DLQ based on queue configuration
        }
    }

    private class SnsMessageWrapper
    {
        public string? Message { get; set; }
    }
}
