using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace aspnetapp.Controllers
{
    public static class Stats
    {
        private static ConcurrentDictionary<string, int> Counter = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);

        public static void Add(string keys)
        {
            Counter.AddOrUpdate(keys, 1, (k, v) => ++v);
        }

        public static void Print()
        {
            var ordered = Counter.OrderByDescending(x => x.Value);

            foreach (var kvp in ordered)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }
    }

}