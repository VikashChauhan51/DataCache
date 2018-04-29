using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataCache
{
    internal interface ICacheData
    {
        DateTime TimeToAlive { get; set; }
    }
    internal class CacheData<T> : ICacheData
    {
        private string jsonData;
        private DateTime timeToAlive;
        public T Item
        {
            get
            {
                return (T)JsonConvert.DeserializeObject(jsonData);
            }

            set
            {
                jsonData = JsonConvert.SerializeObject(value);
            }
        }
        public DateTime TimeToAlive
        {
            get
            {
                return timeToAlive;
            }

            set
            {
                timeToAlive = value;
            }
        }
    }
    public static class InMemoryCache
    {
        private static readonly TimeSpan interval = TimeSpan.FromMinutes(1);
        private static ConcurrentDictionary<string, ICacheData> cache = new ConcurrentDictionary<string, ICacheData>();
        static InMemoryCache()
        {
            CleanUp();
        }

        public static Func<Task<TResult>> Memoize<TResult>(this Func<Task<TResult>> f, DateTime timeToAlive)
        {
            return () => Get(() => f(), f.Method, null, timeToAlive);
        }

        public static Func<T1, Task<TResult>> Memoize<T1, TResult>(this Func<T1, Task<TResult>> f, DateTime timeToAlive)
        {

            return (a1) => Get(() => f(a1), f.Method, new { a1 }, timeToAlive);
        }
        public static Func<T1, T2, Task<TResult>> Memoize<T1, T2, TResult>(this Func<T1, T2, Task<TResult>> f, DateTime timeToAlive)
        {
            return (a1, a2) => Get(() => f(a1, a2), f.Method, new { a1, a2 }, timeToAlive);
        }
        public static Func<T1, T2, T3, Task<TResult>> Memoize<T1, T2, T3, TResult>(this Func<T1, T2, T3, Task<TResult>> f, DateTime timeToAlive)
        {
            return (a1, a2, a3) => Get(() => f(a1, a2, a3), f.Method, new { a1, a2, a3 }, timeToAlive);
        }
        public static Func<T1, T2, T3, T4, Task<TResult>> Memoize<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, Task<TResult>> f, DateTime timeToAlive)
        {
            return (a1, a2, a3, a4) => Get(() => f(a1, a2, a3, a4), f.Method, new { a1, a2, a3, a4 }, timeToAlive);
        }
        public static Func<T1, T2, T3, T4, T5, Task<TResult>> Memoize<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, Task<TResult>> f, DateTime timeToAlive)
        {
            return (a1, a2, a3, a4, a5) => Get(() => f(a1, a2, a3, a4, a5), f.Method, new { a1, a2, a3, a4, a5 }, timeToAlive);
        }

        public static Func<Task> Clear<TResult>(this Func<Task<TResult>> f)
        {
            return () => Delete(f.Method, null);
        }
        public static Func<T1, Task> Clear<T1, TResult>(this Func<T1, Task<TResult>> f)
        {

            return (a1) => Delete(f.Method, new { a1 });
        }
        public static Func<T1, T2, Task> Clear<T1, T2, TResult>(this Func<T1, T2, Task<TResult>> f)
        {
            return (a1, a2) => Delete(f.Method, new { a1, a2 });
        }
        public static Func<T1, T2, T3, Task> Clear<T1, T2, T3, TResult>(this Func<T1, T2, T3, Task<TResult>> f)
        {
            return (a1, a2, a3) => Delete(f.Method, new { a1, a2, a3 });
        }
        public static Func<T1, T2, T3, T4, Task> Clear<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, Task<TResult>> f)
        {
            return (a1, a2, a3, a4) => Delete(f.Method, new { a1, a2, a3, a4 });
        }
        public static Func<T1, T2, T3, T4, T5, Task> Clear<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, Task<TResult>> f)
        {
            return (a1, a2, a3, a4, a5) => Delete(f.Method, new { a1, a2, a3, a4, a5 });
        }
        private static void CleanUp()
        {
            Task.Run(async () =>
            {
                await Task.Delay(interval);
                var keysToRemove = cache.Where(x => x.Value.TimeToAlive < DateTime.Now);
                ICacheData data;
                foreach (var item in keysToRemove)
                    cache.TryRemove(item.Key, out data);
            }).ContinueWith(_ => { CleanUp(); });
        }
        private static string GetKey(MethodInfo method, object arguments)
        {
            var jsonData = JsonConvert.SerializeObject(arguments);
            var signature = $"{method.DeclaringType.FullName}{method.ToString()}";
            return $"{ signature.GetHashCode()}{jsonData.GetHashCode()}";
        }
        private static async Task<T> Get<T>(Func<Task<T>> getter, MethodInfo method, object arguments, DateTime timeToAlive)
        {
            var key = GetKey(method, arguments);
            ICacheData data;
            if (!cache.TryGetValue(key, out data))
            {
                var item = await getter();
                data = new CacheData<T> { TimeToAlive = timeToAlive, Item = item };
                cache.AddOrUpdate(key, data, (oldKey, oldValue) => data);
            }
            return (data as CacheData<T>).Item;
        }
        private static async Task Delete(MethodInfo method, object arguments)
        {
            await Task.Run(() =>
            {
                var key = GetKey(method, arguments);
                ICacheData data;
                cache.TryRemove(key, out data);
            });
        }
    }
}
