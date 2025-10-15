using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EventDispatcher.Concretes;
using EventDispatcher.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class TestEvent : IEvent
{
    public int Id { get; }
    public TestEvent(int id) => Id = id;
}

public class TestEventHandler : IEventHandler<TestEvent>
{
    public int Priority => 0;
    public async Task Handle(TestEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        //await Task.Delay(1);
    }
}

[MemoryDiagnoser]
public class EventDispatcherBenchmark
{
    private ExpressionTreeEventDispatcherAdapter expressionDispatcher = null!;
    private ILogger<ExpressionTreeEventDispatcherAdapter> expressionLogger = null!;

    private ReflectionDispatcherAdapter reflectionDispatcher = null!;
    private ILogger<ReflectionDispatcherAdapter> reflectionLogger = null!;

    private IServiceProvider provider = null!;
    private CancellationToken cancellationToken = CancellationToken.None;
    private List<TestEvent> events = new();

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton<IEventHandler<TestEvent>, TestEventHandler>();

        provider = services.BuildServiceProvider();

        reflectionLogger = provider.GetRequiredService<ILogger<ReflectionDispatcherAdapter>>();
        expressionLogger = provider.GetRequiredService<ILogger<ExpressionTreeEventDispatcherAdapter>>();

        reflectionDispatcher = new ReflectionDispatcherAdapter(provider, reflectionLogger);
        expressionDispatcher = new ExpressionTreeEventDispatcherAdapter(provider, expressionLogger);

        expressionDispatcher.Dispatch(new TestEvent(0), cancellationToken).Wait();

        events = Enumerable.Range(1, 1000)
            .Select(index => new TestEvent(index))
            .ToList();
    }

    [Benchmark]
    public async Task ExpressionTree_Dispatch_1000_Events()
    {
        foreach (var @event in events)
            await expressionDispatcher.Dispatch(@event, cancellationToken);
    }

    [Benchmark]
    public async Task Reflection_Dispatch_1000_Events()
    {
        foreach (var @event in events)
            await reflectionDispatcher.Dispatch(@event, cancellationToken);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<EventDispatcherBenchmark>();
    }
}