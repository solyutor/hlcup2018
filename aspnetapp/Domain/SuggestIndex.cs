using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public static class SuggestIndex
    {
        private static readonly HList<uint>[] suggestions = new HList<uint>[1_400_000];
        private static int _nullified;


        public static bool TryGetIndex(uint meId, out HList<uint> hList)
        {
            hList = suggestions[meId];
            return hList != null;
        }

        public static Task StartIndexing()
        {
            return
            Task.Run( () =>
            {
                DoLikesIndex();
                //DoSuggestIndex();
            });
        }

        private static void DoLikesIndex()
        {
            Console.WriteLine("Indexing likes.");
            foreach (var account in Database.GetAccounts())
            {
                LikesIndexer.Index(account);
            }
            LikesIndexer.Trim();

            Console.WriteLine("Indexed likes.");
        }

        private static void DoSuggestIndex()
        {
         Console.WriteLine("Indexing suggests");
            var watch = Stopwatch.StartNew();

            IReadOnlyCollection<Account> accounts = Database.GetAccounts();

            Parallel.ForEach(accounts, a => IndexSuggest(a, new SuggestQuery{limit = 20}));

            watch.Stop();

            Console.WriteLine($"Suggestion indexing finished in {watch.Elapsed} ({accounts.Count * 1000 /watch.ElapsedMilliseconds} acc/sec)");
        }

        private static void IndexSuggest(Account account, SuggestQuery query)
        {
            if (account == null || account.likes == null || account.likes.Count < 40)
            {
                return;
            }

            query.Reset();
            var result = query.ExecuteWithoutIndex(account);
            if (result.Count == 0)
            {
                suggestions[account.id] = HList<uint>.Empty;
            }
            else
            {
                var clone = new HList<uint>(result.Count);
                clone.AddRange(result.Select(x => x.id));
                suggestions[account.id] = clone;
            }
        }

        public static void Nullify(uint accountId)
        {
            if (suggestions[accountId] != null)
            {
                _nullified++;
                suggestions[accountId] = null;
            }

        }

        public static void PrintNullified()
        {
            Console.WriteLine($"Nullified {_nullified} indexes");
        }
    }
}