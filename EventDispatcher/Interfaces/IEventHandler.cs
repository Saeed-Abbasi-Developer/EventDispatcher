namespace EventDispatcher.Interfaces
{
    public interface IEventHandler<T> where T : IEvent
    {
        int Priority => 0;
        Task Handle(T @event, CancellationToken cancellationToken);
    }
}
