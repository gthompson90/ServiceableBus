using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.SimpleNotificationService;
using NSubstitute;
using ServiceableBus.Aws.Abstractions;
using ServiceableBus.Aws.Options;
using ServiceableBus.Contracts;

namespace ServiceableBus.Aws.Tests
{
    [TestClass]
    public class ServiceablePublisherTests
    {
        private IServiceableBusClientFactory _mockClientFactory = null!;
        private IAmazonSQS _mockSqsClient = null!;
        private IAmazonSimpleNotificationService _mockSnsClient = null!;
        private List<IServiceablePublisherOptions> _options = null!;
        private ServiceablePublisher _publisher = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = Substitute.For<IServiceableBusClientFactory>();
            _mockSqsClient = Substitute.For<IAmazonSQS>();
            _mockSnsClient = Substitute.For<IAmazonSimpleNotificationService>();
            
            _options = new List<IServiceablePublisherOptions>
            {
                new ServiceablePublisherOptions("https://sqs.us-east-1.amazonaws.com/123456789012/test-queue", typeof(TestEvent)),
                new ServiceablePublisherOptions("arn:aws:sns:us-east-1:123456789012:test-topic", typeof(TestTopicEvent))
            };

            _mockClientFactory.GetSqsClient().Returns(_mockSqsClient);
            _mockClientFactory.GetSnsClient().Returns(_mockSnsClient);

            _publisher = new ServiceablePublisher(_mockClientFactory, _options);
        }

        [TestMethod]
        public async Task PublishAsync_ShouldSendMessageToSqs_WhenQueueUrl()
        {
            // Arrange
            var testEvent = new TestEvent { Property = "Test" };
            _mockSqsClient.SendMessageAsync(Arg.Any<SendMessageRequest>(), default)
                .Returns(new SendMessageResponse());

            // Act
            await _publisher.PublishAsync(testEvent);

            // Assert
            await _mockSqsClient.Received(1).SendMessageAsync(Arg.Any<SendMessageRequest>(), default);
        }

        [TestMethod]
        public async Task PublishAsync_ShouldPublishMessageToSns_WhenTopicArn()
        {
            // Arrange
            var testEvent = new TestTopicEvent { Property = "Test" };
            _mockSnsClient.PublishAsync(Arg.Any<Amazon.SimpleNotificationService.Model.PublishRequest>(), default)
                .Returns(new Amazon.SimpleNotificationService.Model.PublishResponse());

            // Act
            await _publisher.PublishAsync(testEvent);

            // Assert
            await _mockSnsClient.Received(1).PublishAsync(Arg.Any<Amazon.SimpleNotificationService.Model.PublishRequest>(), default);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task PublishAsync_ShouldThrowInvalidOperationException_WhenNoOptionsFound()
        {
            // Arrange
            var publisherWithNoOptions = new ServiceablePublisher(_mockClientFactory, new List<IServiceablePublisherOptions>());
            var testEvent = new TestEvent { Property = "Test" };

            // Act
            await publisherWithNoOptions.PublishAsync(testEvent);
        }
    }

    public class TestEvent : IServiceableBusEvent
    {
        public string Property { get; set; } = string.Empty;
    }

    public class TestTopicEvent : IServiceableBusEvent
    {
        public string Property { get; set; } = string.Empty;
    }
}
