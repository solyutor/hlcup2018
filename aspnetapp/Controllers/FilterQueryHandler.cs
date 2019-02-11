using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using aspnetapp.Collections;
using aspnetapp.Extensions;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Http;

namespace aspnetapp.Controllers
{
    public static class FilterHandler
    {
        public static readonly PathString Path = new PathString("/accounts/filter/");

        private static readonly ConcurrentDictionary<string, int> combStor = new ConcurrentDictionary<string, int>(HOrdinalComparer.Instance);
        private static readonly ConcurrentDictionary<string, int> keyStor = new ConcurrentDictionary<string, int>(HOrdinalComparer.Instance);
        private static int count;

        [ThreadStatic] private static FilterQuery _query;

        public static void Process(HttpContext context)
        {
            if (FilterQueryParser.TryParse(context.Request.Query, ref _query))
            {
                context.Response.ContentType = "application/json";
                if (_query.WillYieldZeroResults)
                {
                    context.Response.ContentLength = FastJson.EmptyAccounts.Length;
                    context.Response.Body.Write(FastJson.EmptyAccounts);
                    return;
                }

                HList<Account> result = _query.ExecuteFilter();

                ParseContext stream = Buffer.Context;

                FastJson.WriteFilterResponse(result, _query.Fields, ref stream);

                context.Response.ContentLength = stream.WrittenLength;
                context.Response.Body.Write(stream.WrittenSpan);

                return;
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        private static void LogPredicates(HttpContext context)
        {
            if (context.Request.Query.Count < 4)
            {
                return;
            }

            List<string> keys = context.Request.Query.Keys
                .Where(x => !x.OrdinalEqualsTo("limit") && !x.OrdinalEqualsTo("query_id"))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
            var key = string.Join(',', keys);

            combStor.AddOrUpdate(key, 1, (k, v) => ++v);

            foreach (var key1 in keys)
            {
                keyStor.AddOrUpdate(key1, 1, (k, v) => ++v);
            }


            var current = Interlocked.Increment(ref count);
            if (current == 6500)
            {
                foreach (KeyValuePair<string, int> combination in combStor.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"{combination.Key}: {combination.Value}");
                }

                foreach (KeyValuePair<string, int> combination in keyStor.Where(x => x.Value > 1).OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"{combination.Key}: {combination.Value}");
                }
            }
        }
    }
}