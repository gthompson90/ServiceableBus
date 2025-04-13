using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using NSubstitute;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Options;

namespace ServiceableBus.Azure.Tests
{
    [TestClass]
    public class ServiceableBusBackgroundServiceTests
    {
        private ServiceableBusBackgroundService _backgroundService;
        private IServiceProvider _mockServiceProvider;
        private IOptions<ServiceableBusOptions> _mockOptions;
        private List<IServiceableListener> _mockListeners;

        [TestInitialize]
        public void Setup()
        {
            _mockOptions = Substitute.For<IOptions<ServiceableBusOptions>>();
            _mockServiceProvider = Substitute.For<IServiceProvider>();
            _mockOptions.Value.Returns(new ServiceableBusOptions { ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=testkey" });

            _mockListeners = new List<IServiceableListener>
            {
                Substitute.For<IServiceableListener>(),
                Substitute.For<IServiceableListener>()
            };

            _backgroundService = new ServiceableBusBackgroundService(_mockServiceProvider, _mockOptions, _mockListeners);
        }

        [TestMethod]
        public async Task StartAsync_ShouldStartAllListeners()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _backgroundService.StartAsync(cancellationToken);

            // Assert
            foreach (var listener in _mockListeners)
            {
                await listener.Received(1).StartProcessor(Arg.Any<ServiceBusClient>(), cancellationToken);
            }
        }

        [TestMethod]
        public async Task StopAsync_ShouldStopAllListeners()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _backgroundService.StopAsync(cancellationToken);

            // Assert
            foreach (var listener in _mockListeners)
            {
                await listener.Received(1).StopProcessor(cancellationToken);
                await listener.Received(1).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task DisposeAsync_ShouldDisposeAllListenersAndClient()
        {
            // Act
            await _backgroundService.DisposeAsync();

            // Assert
            foreach (var listener in _mockListeners)
            {
                await listener.Received(1).DisposeAsync();
            }
        }
    }
}