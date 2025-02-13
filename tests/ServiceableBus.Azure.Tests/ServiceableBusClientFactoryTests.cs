using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using NSubstitute;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Options;

namespace ServiceableBus.Azure.Tests
{
    [TestClass]
    public class ServiceableBusClientFactoryTests
    {
        private IOptions<ServiceableBusOptions> _mockOptions;
        private ServiceableBusOptions _serviceableBusOptions;
        private ServiceableBusClientFactory _clientFactory;

        [TestInitialize]
        public void Setup()
        {
            _serviceableBusOptions = new ServiceableBusOptions
            {
                ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=testkey"
            };

            _mockOptions = Substitute.For<IOptions<ServiceableBusOptions>>();
            _mockOptions.Value.Returns(_serviceableBusOptions);

            _clientFactory = new ServiceableBusClientFactory(_mockOptions);
        }

        [TestMethod]
        public void CreateSender_ShouldReturnServiceBusSender()
        {
            // Arrange
            var mockPublisherOptions = Substitute.For<IServiceablePublisherOptions>();
            mockPublisherOptions.QueueName.Returns("test-queue");

            // Act
            var sender = _clientFactory.CreateSender(mockPublisherOptions);

            // Assert
            Assert.IsNotNull(sender);
            Assert.IsInstanceOfType(sender, typeof(ServiceBusSender));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateSender_ShouldThrowArgumentNullException_WhenOptionsIsNull()
        {
            // Act
            _clientFactory.CreateSender(null);
        }
    }
}

