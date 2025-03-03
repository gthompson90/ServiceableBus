using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Listeners;
using ServiceableBus.Contracts;
using System.Text;
using System.Text.Json;

namespace ServiceableBus.Azure.Tests
{
    [TestClass]
    public class ServiceableQueueListenerTests
    {
        private IServiceProvider _mockServiceProvider;
        private IServiceableQueueListenerOptions<TestEvent> _mockOptions;
        private IServiceableRetryOptions _mockRetryOptions;
        private ServiceBusClient _mockClient;
        private ServiceableQueueListener<TestEvent> _listener;
        private ServiceBusReceiver _mockReceiver;

        [TestInitialize]
        public void Setup()
        {
            _mockServiceProvider = Substitute.For<IServiceProvider>();
            _mockOptions = Substitute.For<IServiceableQueueListenerOptions<TestEvent>>();
            _mockClient = Substitute.For<ServiceBusClient>();
            _mockReceiver = Substitute.For<ServiceBusReceiver>();
            _mockRetryOptions = Substitute.For<IServiceableRetryOptions>();

            _mockOptions.QueueName.Returns("test-queue");

            _listener = new ServiceableQueueListener<TestEvent>(_mockServiceProvider, _mockOptions, _mockRetryOptions);
        }

        [TestMethod]
        public async Task StartProcessor_ShouldStartProcessing()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var mockProcessor = Substitute.For<ServiceBusProcessor>();

            _mockClient.CreateProcessor(_mockOptions.QueueName).Returns(mockProcessor);

            // Act
            await _listener.StartProcessor(_mockClient, cancellationToken);

            // Assert
            await mockProcessor.Received(1).StartProcessingAsync(cancellationToken);
        }

        [TestMethod]
        public async Task StopProcessor_ShouldStopProcessing()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var mockProcessor = Substitute.For<ServiceBusProcessor>();

            _mockClient.CreateProcessor(_mockOptions.QueueName).Returns(mockProcessor);
            await _listener.StartProcessor(_mockClient, cancellationToken);

            // Act
            await _listener.StopProcessor(cancellationToken);

            // Assert
            await mockProcessor.Received(1).StopProcessingAsync(cancellationToken);
            await mockProcessor.Received(1).DisposeAsync();
        }

        [TestMethod]
        public async Task ProcessMessageAsync_ShouldHandleMessage()
        {
            _mockReceiver.CompleteMessageAsync(
                Arg.Any<ServiceBusReceivedMessage>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var testEvent = new TestEvent { Property = "Test" };
            var messageBody = JsonSerializer.Serialize(testEvent);
            var messageBytes = Encoding.UTF8.GetBytes(messageBody);

            ServiceBusReceivedMessage message = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBytes),
                messageId: "messageId"
            );

            ProcessMessageEventArgs processArgs = new(
                message: message,
                receiver: _mockReceiver,
                cancellationToken: CancellationToken.None);

            // Arrange
            var mockEventHandler = Substitute.For<IServiceableBusEventHandler<TestEvent>>();
            var mockScope = Substitute.For<IServiceScope>();
            var mockScopeFactory = Substitute.For<IServiceScopeFactory>();

            _mockServiceProvider.GetService(typeof(IServiceScopeFactory)).Returns(mockScopeFactory);
            mockScopeFactory.CreateScope().Returns(mockScope);
            mockScope.ServiceProvider.GetService(typeof(IServiceableBusEventHandler<TestEvent>)).Returns(mockEventHandler);

            // Act
            await _listener.ProcessMessageAsync<TestEvent>(processArgs);

            // Assert
            await mockEventHandler.Received(1).Handle(Arg.Is<TestEvent>(e => e.Property == "Test"), Arg.Any<ServiceablePropertyBag>());
        }

        [TestMethod]
        public void Dispose_ShouldDisposeProcessor()
        {
            // Arrange
            var mockProcessor = Substitute.For<ServiceBusProcessor>();

            _mockClient.CreateProcessor(_mockOptions.QueueName).Returns(mockProcessor);
            _listener.StartProcessor(_mockClient, CancellationToken.None).Wait();

            // Act
            _listener.Dispose();

            // Assert
            mockProcessor.Received(1).DisposeAsync();
        }
    }
}


