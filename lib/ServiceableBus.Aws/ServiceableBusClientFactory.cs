using Amazon.SQS;
using Amazon.SimpleNotificationService;
using Amazon;
using Microsoft.Extensions.Options;
using ServiceableBus.Aws.Abstractions;
using ServiceableBus.Aws.Options;
using Amazon.Runtime;

namespace ServiceableBus.Aws;

internal class ServiceableBusClientFactory : IServiceableBusClientFactory
{
    private readonly ServiceableBusOptions _serviceableBusOptions;
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonSimpleNotificationService _snsClient;

    public ServiceableBusClientFactory(IOptions<ServiceableBusOptions> serviceableBusOptions)
    {
        _serviceableBusOptions = serviceableBusOptions.Value;
        
        var region = RegionEndpoint.GetBySystemName(_serviceableBusOptions.Region);
        
        if (!string.IsNullOrEmpty(_serviceableBusOptions.AccessKey) && 
            !string.IsNullOrEmpty(_serviceableBusOptions.SecretKey))
        {
            var credentials = new BasicAWSCredentials(_serviceableBusOptions.AccessKey, _serviceableBusOptions.SecretKey);
            _sqsClient = new AmazonSQSClient(credentials, region);
            _snsClient = new AmazonSimpleNotificationServiceClient(credentials, region);
        }
        else
        {
            _sqsClient = new AmazonSQSClient(region);
            _snsClient = new AmazonSimpleNotificationServiceClient(region);
        }
    }

    public IAmazonSQS GetSqsClient() => _sqsClient;
    
    public IAmazonSimpleNotificationService GetSnsClient() => _snsClient;
}
