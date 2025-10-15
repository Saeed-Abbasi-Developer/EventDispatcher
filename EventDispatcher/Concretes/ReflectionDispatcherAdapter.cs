using EventDispatcher.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EventDispatcher.Concretes
{
    public class ReflectionDispatcherAdapter : IEventDispatcher
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ReflectionDispatcherAdapter> logger;

        public ReflectionDispatcherAdapter(IServiceProvider serviceProvider, ILogger<ReflectionDispatcherAdapter> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task Dispatch(IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
        {
            foreach (var @event in events)
                await Dispatch(@event, cancellationToken);
        }

        public async Task Dispatch(IEvent @event, CancellationToken cancellationToken)
        {
            var eventType = @event.GetType();

            var handleEventMethod = typeof(ReflectionDispatcherAdapter)
                .GetMethod(nameof(HandleEvent), BindingFlags.Instance | BindingFlags.Public)!
                .MakeGenericMethod(eventType);

            var task = (Task)handleEventMethod.Invoke(this, new object[] { @event, cancellationToken })!;

            await task;
        }

        public async Task HandleEvent<T>(T @event, CancellationToken cancellationToken) where T : IEvent
        {
            var handlers = serviceProvider.GetServices<IEventHandler<T>>();

            foreach (var handler in handlers)
                await handler.Handle(@event, cancellationToken);
        }
    }
}
