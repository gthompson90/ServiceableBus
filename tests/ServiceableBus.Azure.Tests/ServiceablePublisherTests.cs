using Azure.Messaging.ServiceBus;
using NSubstitute;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Options;
using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Tests
{
    [TestClass]
    public class ServiceablePublisherTests
    {
        private IServiceableBusClientFactory _mockClientFactory;
        private ServiceBusSender _mockSender;
        private List<IServiceablePublisherOptions> _options;
        private ServiceablePublisher _publisher;

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = Substitute.For<IServiceableBusClientFactory>();
            _mockSender = Substitute.For<ServiceBusSender>();
            _options = new List<IServiceablePublisherOptions>
            {
                new ServiceablePublisherOptions("test-queue", typeof(TestEvent))
            };

            _mockClientFactory.CreateSender(Arg.Any<IServiceablePublisherOptions>()).Returns(_mockSender);

            _publisher = new ServiceablePublisher(_mockClientFactory, _options);
        }

        [TestMethod]
        public async Task PublishAsync_ShouldSendMessage()
        {
            // Arrange
            var testEvent = new TestEvent { Property = "Test" };

            // Act
            await _publisher.PublishAsync(testEvent);

            // Assert
            await _mockSender.Received(1).SendMessageAsync(Arg.Any<ServiceBusMessage>(), default);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task PublishAsync_ShouldThrowInvalidOperationException_WhenSenderIsNull()
        {
            // Arrange
            _mockClientFactory.CreateSender(Arg.Any<IServiceablePublisherOptions>()).Returns((ServiceBusSender)null);

            var testEvent = new TestEvent { Property = "Test" };

            // Act
            await _publisher.PublishAsync(testEvent);
        }
    }

    public class TestEvent : IServiceableBusEvent
    {
        public string Property { get; set; }
    }
}

