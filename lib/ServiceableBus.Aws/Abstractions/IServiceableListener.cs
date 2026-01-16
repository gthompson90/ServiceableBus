namespace ServiceableBus.Aws.Abstractions;

public interface IServiceableListener : IDisposable
{
    public Type EventType { get; }
    public Task StartProcessor(CancellationToken cancellationToken);
    public Task StopProcessor(CancellationToken cancellationToken);
}
