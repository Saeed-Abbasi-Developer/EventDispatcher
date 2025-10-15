using EventDispatcher.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EventDispatcher.Concretes
{
    public class ExpressionTreeEventDispatcherAdapter : IEventDispatcher
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ExpressionTreeEventDispatcherAdapter> logger;

        private readonly ConcurrentDictionary<Type, Func<IEvent, CancellationToken, Task>> eventDelegatesCache = new();

        private readonly ConcurrentDictionary<Type, object> handlersCache = new();

        public ExpressionTreeEventDispatcherAdapter(IServiceProvider serviceProvider, ILogger<ExpressionTreeEventDispatcherAdapter> logger)
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
            Type eventType = @event.GetType();

            var handlerDelegate = eventDelegatesCache.GetOrAdd(eventType, key =>
            {
                var eventParamExpression = Expression.Parameter(typeof(IEvent), "@event");
                var cancellationTokenParamExpression = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                var handleEventMethod = typeof(ExpressionTreeEventDispatcherAdapter).GetMethod(nameof(HandleEvent)) ??
                    throw new NullReferenceException($"Method {nameof(HandleEvent)} not found");

                var genericHandleEventMethod = handleEventMethod.MakeGenericMethod(eventType);

                var dispatcherInstance = Expression.Constant(this);

                var call = Expression.Call(dispatcherInstance, genericHandleEventMethod,
                    Expression.Convert(eventParamExpression, eventType),
                    cancellationTokenParamExpression);

                var lambdaExpression = Expression.Lambda<Func<IEvent, CancellationToken, Task>>(call, eventParamExpression, cancellationTokenParamExpression);

                return lambdaExpression.Compile();
            });

            await handlerDelegate(@event, cancellationToken);
        }

        public async Task HandleEvent<T>(T @event, CancellationToken cancellationToken) where T : IEvent
        {
            var handlers = GetHandlers<T>();

            foreach (var handler in handlers)
                await handler.Handle(@event, cancellationToken);
        }

        private List<IEventHandler<T>> GetHandlers<T>() where T : IEvent
        {
            var handlers = (List<IEventHandler<T>>)handlersCache.GetOrAdd(typeof(T), eventType =>
            {
                return serviceProvider.GetServices<IEventHandler<T>>()
                    .OrderBy(handler => handler.Priority)
                    .ToList();
            });

            return handlers;
        }
    }
}
