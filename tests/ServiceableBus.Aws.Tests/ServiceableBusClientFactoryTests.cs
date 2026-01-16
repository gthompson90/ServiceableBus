using Amazon.SQS;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Options;
using NSubstitute;
using ServiceableBus.Aws.Options;

namespace ServiceableBus.Aws.Tests
{
    [TestClass]
    public class ServiceableBusClientFactoryTests
    {
        private IOptions<ServiceableBusOptions> _mockOptions = null!;
        private ServiceableBusOptions _serviceableBusOptions = null!;
        private ServiceableBusClientFactory _clientFactory = null!;

        [TestInitialize]
        public void Setup()
        {
            _serviceableBusOptions = new ServiceableBusOptions
            {
                Region = "us-east-1"
            };

            _mockOptions = Substitute.For<IOptions<ServiceableBusOptions>>();
            _mockOptions.Value.Returns(_serviceableBusOptions);

            _clientFactory = new ServiceableBusClientFactory(_mockOptions);
        }

        [TestMethod]
        public void GetSqsClient_ShouldReturnAmazonSQSClient()
        {
            // Act
            var sqsClient = _clientFactory.GetSqsClient();

            // Assert
            Assert.IsNotNull(sqsClient);
            Assert.IsInstanceOfType(sqsClient, typeof(IAmazonSQS));
        }

        [TestMethod]
        public void GetSnsClient_ShouldReturnAmazonSimpleNotificationServiceClient()
        {
            // Act
            var snsClient = _clientFactory.GetSnsClient();

            // Assert
            Assert.IsNotNull(snsClient);
            Assert.IsInstanceOfType(snsClient, typeof(IAmazonSimpleNotificationService));
        }

        [TestMethod]
        public void Constructor_ShouldCreateClientsWithCredentials_WhenAccessKeyAndSecretKeyProvided()
        {
            // Arrange
            var optionsWithCredentials = new ServiceableBusOptions
            {
                Region = "us-east-1",
                AccessKey = "AKIAIOSFODNN7EXAMPLE",
                SecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
            };

            var mockOptionsWithCreds = Substitute.For<IOptions<ServiceableBusOptions>>();
            mockOptionsWithCreds.Value.Returns(optionsWithCredentials);

            // Act
            var factory = new ServiceableBusClientFactory(mockOptionsWithCreds);
            var sqsClient = factory.GetSqsClient();
            var snsClient = factory.GetSnsClient();

            // Assert
            Assert.IsNotNull(sqsClient);
            Assert.IsNotNull(snsClient);
        }
    }
}
