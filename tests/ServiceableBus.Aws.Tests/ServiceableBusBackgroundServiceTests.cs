using Microsoft.Extensions.Hosting;
using NSubstitute;
using ServiceableBus.Aws.Abstractions;

namespace ServiceableBus.Aws.Tests
{
    [TestClass]
    public class ServiceableBusBackgroundServiceTests
    {
        private ServiceableBusBackgroundService _backgroundService = null!;
        private List<IServiceableListener> _mockListeners = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockListeners = new List<IServiceableListener>
            {
                Substitute.For<IServiceableListener>(),
                Substitute.For<IServiceableListener>()
            };

            _backgroundService = new ServiceableBusBackgroundService(_mockListeners);
        }

        [TestMethod]
        public async Task StartAsync_ShouldStartAllListeners()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _backgroundService.StartAsync(cancellationToken);

            // Assert
            foreach (var listener in _mockListeners)
            {
                await listener.Received(1).StartProcessor(cancellationToken);
            }
        }

        [TestMethod]
        public async Task StopAsync_ShouldStopAllListeners()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _backgroundService.StopAsync(cancellationToken);

            // Assert
            foreach (var listener in _mockListeners)
            {
                await listener.Received(1).StopProcessor(cancellationToken);
                listener.Received(1).Dispose();
            }
        }

        [TestMethod]
        public async Task DisposeAsync_ShouldDisposeAllListeners()
        {
            // Act
            await _backgroundService.DisposeAsync();

            // Assert
            foreach (var listener in _mockListeners)
            {
                listener.Received(1).Dispose();
            }
        }
    }
}
