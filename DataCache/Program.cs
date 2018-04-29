using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCache
{
    class MyClass
    {

        public async Task<string> GetMessage()
        {

            return await Task.FromResult("Hello");
        }
        public async Task<string> GetMessage(string message)
        {

            return await Task.FromResult(message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var obj = new MyClass();
            var data = InMemoryCache.Memoize<string, string>(obj.GetMessage, DateTime.Now.AddMinutes(1))("demo");
            Console.WriteLine(data.Result);
            InMemoryCache.Clear<string, string>(obj.GetMessage)("demo").Wait();
            var data1 = InMemoryCache.Memoize(obj.GetMessage, DateTime.Now.AddMinutes(1))();
            Console.WriteLine(data1.Result);
        }
    }
}
