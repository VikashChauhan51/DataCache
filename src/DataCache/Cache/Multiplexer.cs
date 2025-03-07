using System.Collections.Concurrent;

namespace DataCache.Cache;

internal class Multiplexer : IDisposable
{
    private readonly BlockingCollection<Func<Task>> _tasks = new(new ConcurrentQueue<Func<Task>>());
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _processingTask;
    public Multiplexer()
    {
        _processingTask = Task.Run(ProcessTaskAsync, _cancellationTokenSource.Token);

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
        _cancellationTokenSource.Dispose();
        _processingTask.Dispose();
        _tasks.Dispose();
    }
    private async Task ProcessTaskAsync()
    {
        foreach (var task in _tasks.GetConsumingEnumerable(_cancellationTokenSource.Token))
        {
            await task();
        }
    }
}
