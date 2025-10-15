namespace EventDispatcher.Interfaces
{
    public interface IEventDispatcher
    {
        Task Dispatch(IReadOnlyList<IEvent> events, CancellationToken cancellationToken);
        Task Dispatch(IEvent @event, CancellationToken cancellationToken);
    }
}
