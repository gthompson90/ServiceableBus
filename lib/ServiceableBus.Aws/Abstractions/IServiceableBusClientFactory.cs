using Amazon.SQS;
using Amazon.SimpleNotificationService;

namespace ServiceableBus.Aws.Abstractions
{
    public interface IServiceableBusClientFactory
    {
        public IAmazonSQS GetSqsClient();
        public IAmazonSimpleNotificationService GetSnsClient();
    }
}
