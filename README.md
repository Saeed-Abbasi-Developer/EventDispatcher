# EventDispatcher Benchmark 🚀

A high-performance tool for handling events using two approaches: **Reflection** and **Expression Trees**.  
It includes a benchmark to compare performance and memory usage.

---

## 📑 Table of Contents

- [Description](#description)  
- [Features](#features)  
- [Installation](#installation)  
- [Usage Example](#usage-example)  
- [How It Works](#how-it-works)  
- [Run Benchmark](#run-benchmark)  
- [Benchmark Results](#benchmark-results)  

---

## 📝 Description

EventDispatcher is a library designed to dispatch events efficiently in **Domain-Driven Design (DDD)** or **event-driven systems**.  
It supports dispatching events either using **Reflection** or **Expression Trees**, with the latter offering improved performance after initial compilation.

This project includes a benchmark to measure **execution speed** and **memory usage** for both approaches.

---

## ✨ Features

- Dispatch events asynchronously  
- Support multiple handlers per event  
- Execute handlers in order based on the `Priority` property  
- Compare performance and memory usage of Reflection vs Expression Trees  

---

## ⚙️ Installation

Clone the repository and restore dependencies:

```bash
git clone https://github.com/Saeed-Abbasi-Developer/EventDispatcher.git
cd EventDispatcher
dotnet restore
```

---

## 💻 Usage Example

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IEventHandler<TestEvent>, TestEventHandler>();

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<ExpressionTreeEventDispatcherAdapter>>();

var dispatcher = new ExpressionTreeEventDispatcherAdapter(provider, logger);

// Dispatch a single event
await dispatcher.Dispatch(new TestEvent(1), CancellationToken.None);
```

---

## 🔍 How It Works

- **ReflectionDispatcherAdapter**: Uses reflection to invoke the `HandleEvent` method for the specific event type at runtime.  
- **ExpressionTreeEventDispatcherAdapter**: Generates a compiled delegate using expression trees for each event type, improving performance for repeated dispatches.  
- Handlers are retrieved from `IServiceProvider` and executed in order of priority.

---

## 🏁 Run Benchmark

This project uses **BenchmarkDotNet** to compare performance and memory usage:

```bash
dotnet run -c Release
```

It will dispatch **1000 events** using both **Expression Trees** and **Reflection**, and measure execution time and memory allocation.

---

## 📊 Benchmark Results

Below is a sample benchmark for dispatching 1000 events:

| Method                              | Mean     | Error    | StdDev   | Gen0    | Allocated |
|------------------------------------ |---------:|---------:|---------:|--------:|----------:|
| ExpressionTree_Dispatch_1000_Events | 122.6 us |  1.04 us |  0.93 us | 38.0859 | 156.25 KB |
| Reflection_Dispatch_1000_Events     | 797.4 us | 15.84 us | 26.90 us | 70.3125 | 289.06 KB |

> ✅ Expression Trees are significantly faster and use less memory after initial compilation.  

> ⚠️ Note: Replace this table and the corresponding graph with your actual benchmark results for accurate visualization.

---

📄 License

MIT License © Saeed Abbasi
