using System.Collections.Concurrent;

namespace DataCache.Cache;

internal class Multiplexer : IDisposable
{
    private readonly BlockingCollection<Func<Task>> _tasks = new(new ConcurrentQueue<Func<Task>>());
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Thread _processingTask;
    public Multiplexer()
    {
        _processingTask = new Thread(ProcessTask)
        {
            IsBackground = true
        };

        _processingTask.Start();

    }

    public Task EnqueueAsync(Action action)
    {
        var tcs = new TaskCompletionSource();
        _tasks.Add(async () =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

        });
        return tcs.Task;
    }
    public Task<T?> EnqueueAsync<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        _tasks.Add(async () =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

        });
        return tcs.Task;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _tasks.CompleteAdding();
        _processingTask.Join();
        _cancellationTokenSource.Dispose();
        _tasks.Dispose();
    }
    private void ProcessTask()
    {
        foreach (var task in _tasks.GetConsumingEnumerable(_cancellationTokenSource.Token))
        {
            task().Wait();
        }
    }
}
