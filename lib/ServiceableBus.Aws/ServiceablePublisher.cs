using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.SimpleNotificationService;
using ServiceableBus.Aws.Abstractions;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SnsMessageAttributeValue = Amazon.SimpleNotificationService.Model.MessageAttributeValue;
using SqsMessageAttributeValue = Amazon.SQS.Model.MessageAttributeValue;

namespace ServiceableBus.Aws;

internal class ServiceablePublisher : IServiceablePublisher
{
    private readonly IServiceableBusClientFactory _clientFactory;
    private readonly IEnumerable<IServiceablePublisherOptions> _options;

    public ServiceablePublisher(IServiceableBusClientFactory clientFactory, IEnumerable<IServiceablePublisherOptions> options)
    {
        _clientFactory = clientFactory;
        _options = options;
    }

    public async Task PublishAsync<T>(T message, Func<ServiceablePropertyBag>? action = null) where T : IServiceableBusEvent
    {
        var messageOptions = _options.FirstOrDefault(x => x.MessageType == typeof(T));

        if (messageOptions is null)
            throw new InvalidOperationException($"No publisher options found for message type {typeof(T).Name}.");

        var options = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            IncludeFields = true,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        var eventInstance = JsonSerializer.Serialize(message, options);

        // Determine if it's a queue (SQS) or topic (SNS) based on the name pattern
        if (messageOptions.QueueOrTopicName.Contains("arn:aws:sns:") || messageOptions.QueueOrTopicName.StartsWith("arn:"))
        {
            // Publish to SNS Topic
            var snsClient = _clientFactory.GetSnsClient();
            var publishRequest = new Amazon.SimpleNotificationService.Model.PublishRequest
            {
                TopicArn = messageOptions.QueueOrTopicName,
                Message = eventInstance,
                MessageAttributes = new Dictionary<string, SnsMessageAttributeValue>()
            };

            if (action is not null)
            {
                foreach (var (key, value) in action.Invoke().ToDictionary())
                {
                    publishRequest.MessageAttributes.Add(key, new SnsMessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = value?.ToString() ?? string.Empty
                    });
                }
            }

            await snsClient.PublishAsync(publishRequest);
        }
        else
        {
            // Send to SQS Queue
            var sqsClient = _clientFactory.GetSqsClient();
            var sendRequest = new SendMessageRequest
            {
                QueueUrl = messageOptions.QueueOrTopicName,
                MessageBody = eventInstance,
                MessageAttributes = new Dictionary<string, SqsMessageAttributeValue>()
            };

            if (action is not null)
            {
                foreach (var (key, value) in action.Invoke().ToDictionary())
                {
                    sendRequest.MessageAttributes.Add(key, new SqsMessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = value?.ToString() ?? string.Empty
                    });
                }
            }

            await sqsClient.SendMessageAsync(sendRequest);
        }
    }
}
