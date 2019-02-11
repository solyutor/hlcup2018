using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using aspnetapp.Domain;
using aspnetapp.Loader;
using aspnetapp.Sys;

namespace aspnetapp
{
    public static class Database
    {
        public static readonly Account[] AccountTable = new Account[Constants.MaxAccountNumber];

        public static readonly HashSet<SpanEmail> emails = new HashSet<SpanEmail>(Constants.MaxAccountNumber);

        private static uint _maxId;
        private static bool _printStats = Environment.MachineName == "BLACKBOX" || Environment.MachineName == "ULTIMA";

        public static int Now;

        private static Stack<Account> _newAccounts = new Stack<Account>(25_000);

        public static Email MinEmail;
        public static Email MaxIEmail;

        public static void Init(string path = null)
        {
            var containerPath = "/tmp/data/data.zip";
            var file = File.Exists(containerPath)
                ? containerPath
                : Path.GetFullPath("data.zip");
            if (path != null)
            {
                file = path;
            }

            LoadAccounts(file);
            Program.Collect(true);
            Reindex();
        }

        private static void LoadAccounts(string file)
        {
            using (var loader = new ZipLoader(file))
            {
                Now = loader.GetNow();
                float totalCount = loader.GetCount();
                var loaded = 0;

                Console.WriteLine($"Starting loading {totalCount} accounts.");
                var collection = new BlockingCollection<AccountStub>();

                loader.GetAccounts(collection);

                var watch = Stopwatch.StartNew();
                foreach (AccountStub account in collection.GetConsumingEnumerable())
                {
                    InsertFromBeginning(account);
                    loaded++;
                    if (_printStats && loaded % 10000 == 0)
                    {
                        Console.Write($"\rLoaded {loaded}/{totalCount}. ({100 * loaded / totalCount:N2}%)");
                    }
                }

                watch.Stop();

                Console.WriteLine($"Loading speed {totalCount / watch.ElapsedMilliseconds:N3} account/ms");
            }
        }


        private static void Reindex()
        {
            var maxLikeCount = 0;
            Console.WriteLine("Starting indexing");
            var indexed = 0;
            var suggestIndexTask = SuggestIndex.StartIndexing();

            MinEmail = AccountTable[1].Email;
            MaxIEmail = AccountTable[1].Email;

            foreach (Account account in GetAccounts())
            {
                if (account.likes != null && account.likes.Count > 0)
                {
                    maxLikeCount = Math.Max(maxLikeCount, account.likes.Max(x => x.Count));
                }
                IndexAccount(account, false);
                indexed++;

                SetMinMax(account.Email);

                if (_printStats && indexed % 10000 == 0)
                {
                    Console.Write($"\rIndexed {indexed}/{_maxId}. ({100.0 * indexed/ _maxId:N2}%)");
                }
            }

            StringIndexer.TrimAll();
            BirthIndex.Trim();
            JoinedIndex.Trim();

            GroupIndex.Trim();
            CreateAccountsForNewRequests();
            suggestIndexTask.Wait();
        }

        private static void CreateAccountsForNewRequests()
        {
            for (int i = 0; i < 25_000; i++)
            {
                _newAccounts.Push(new Account());
            }
        }

        public static void IndexAccount(Account account, bool indexLikes)
        {
            if (indexLikes)
            {
                LikesIndexer.Index(account);
            }

            StringIndexer.Cities.Index(account, account.cityIndex);
            StringIndexer.Countries.Index(account, account.countryIndex);
            StringIndexer.FirstNames.Index(account, account.fnameIndex);
            StringIndexer.LastNames.Index(account, account.snameIndex);

            StringIndexer.Domains.Index(account, account.Email._domain);

            foreach (var interest in account.InterestIndexes)
            {
                StringIndexer.Interests.Index(account, interest);
            }

            BirthIndex.Index(account);
            JoinedIndex.Index(account);

            GroupIndex.Index(account);
            JoinedInterestIndex.Index(account);

            RecommendIndex.Index(account);
        }

        public static IReadOnlyCollection<Account> GetAccounts()
        {
            return new BackwardArray<Account>(AccountTable, 1, _maxId);
        }

        public static IReadOnlyCollection<uint> GetAccountIds()
        {
            return new BackwardRange(_maxId);
        }

        public static bool NotExists(uint accountId)
            => AccountTable.Length <= accountId || AccountTable[accountId] == null;

        public static bool IsUniqueEmail(SpanEmail email)
            => !emails.Contains(email);

        public static bool TryGetAccount(uint accountId, out Account account)
        {
            if (AccountTable.Length <= accountId)
            {
                account = null;
                return false;
            }
            account = AccountTable[accountId];
            return account != null;
        }

        public static void InsertFromBeginning(AccountStub stub)
        {
            var account = new Account();
            ProcessInsert(stub, account);
            account.likes?.TrimExcess();
        }
        public static Account InsertFromPost(AccountStub stub)
        {
            var account = _newAccounts.Pop();
            return ProcessInsert(stub, account);
        }

        private static Account ProcessInsert(AccountStub stub, Account account)
        {
            stub.FillAccount(account);

            AccountTable[account.id] = account;

            emails.Add(account.Email);

            _maxId = Math.Max(_maxId, account.id);
            return account;
        }


        public static Account GetAccount(uint accountId)
        {
            return AccountTable[accountId];
        }

        public static void Replace(in Email old, in Email @new)
        {
            SetMinMax(@new);

            emails.Remove(old);
            emails.Add(@new);
        }

        private static void SetMinMax(Email @new)
        {
            if (@new < MinEmail)
            {
                MinEmail = @new;
            }

            if (@new > MaxIEmail)
            {
                MaxIEmail = @new;
            }
        }
    }
}