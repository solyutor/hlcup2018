using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using aspnetapp.Domain;
using aspnetapp.Sys;

namespace aspnetapp.Serializer
{
    public static class UpdateWorker
    {
        private static readonly Thread _worker;
        private static readonly BlockingCollection<object> _updateQueue;


        static UpdateWorker()
        {
            _updateQueue = new BlockingCollection<object>();

            _worker = new Thread(ProcessQueue)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

        }

        public static void Init()
        {
            var obj = new object();
            foreach (var i in Enumerable.Range(1, 25_000))
            {
                _updateQueue.Add(obj);
            }
            _worker.Start();
        }


        public static void Update(in AccountStub stub) => _updateQueue.Add(stub);

        public static void AddNew(Account account) => _updateQueue.Add(account);

        public static void AddLikes(HList<NewLike> result) => _updateQueue.Add(result);


        private static void ProcessQueue()
        {
            foreach (var obj in _updateQueue.GetConsumingEnumerable())
            {
                try
                {
                    ProcessInternal(obj);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }

        private static void ProcessInternal(object obj)
        {
            if (obj is Account account)
            {
                if (account.likes != null && account.likes.Count != 0)
                {
                    foreach (Like like in account.likes)
                    {
                        SuggestIndex.Nullify(like.Id);
                    }
                }

                Database.IndexAccount(account, true);
            }
            else if (obj is AccountStub stub)
            {
                Reindex(stub);
            }
            else if (obj is HList<NewLike> newLikes)
            {
                AddNewLikes(newLikes);
            }

        }

        private static void AddNewLikes(HList<NewLike> newLikes)
        {
            foreach (NewLike newLike in newLikes)
            {
                SuggestIndex.Nullify(newLike.Likee);
                SuggestIndex.Nullify(newLike.Liker);

                Account liker = Database.GetAccount(newLike.Liker);

                var stub = new LikeStub {id = newLike.Likee, ts = newLike.Ts};
                if (liker.AddLike(stub, out Like like))
                {
                    LikesIndexer.IndexLike(liker, like);
                }
            }

            UpdatePool.Return(newLikes);
        }

        private static void Reindex(in AccountStub stub)
        {
            Account account = Database.GetAccount(stub.id);
            UpdatedFields fields = stub.Fields;
            RemoveFromIndexes(account, fields);



            if ((fields & UpdatedFields.Email) == UpdatedFields.Email)
            {
                account.Email = stub.email;
            }

            if ((fields & UpdatedFields.FName) == UpdatedFields.FName)
            {
                account.fname = stub.fname;
            }

            if ((fields & UpdatedFields.SName) == UpdatedFields.SName)
            {
                account.sname = stub.sname;
            }

            if ((fields & UpdatedFields.Phone) == UpdatedFields.Phone)
            {
                account.phone = UnsafeStringContainer.GetString(stub.phone, true);
            }

            if ((fields & UpdatedFields.Country) == UpdatedFields.Country)
            {
                account.country = stub.country;
            }

            if ((fields & UpdatedFields.City) == UpdatedFields.City)
            {
                account.city = stub.city;
            }

            if ((fields & UpdatedFields.Interests) == UpdatedFields.Interests)
            {
                account.interests = stub.interests;
            }

            if ((fields & UpdatedFields.Premium) == UpdatedFields.Premium)
            {
                account.premium = stub.premium;
            }

            if ((fields & UpdatedFields.Status) == UpdatedFields.Status)
            {
                account.SexStatus &= SexStatus.AllSex;
                account.SexStatus |= stub.sexStatus & SexStatus.AllStatus;
            }

            if ((fields & UpdatedFields.Sex) == UpdatedFields.Sex)
            {
                account.SexStatus &= SexStatus.AllStatus;
                account.SexStatus |= stub.sexStatus & SexStatus.AllSex;
            }


            if ((fields & UpdatedFields.Birth) == UpdatedFields.Birth)
            {
                account.birth = stub.birth;
            }

            if ((fields & UpdatedFields.Joined) == UpdatedFields.Joined)
            {
                account.joined = stub.joined;
            }

            if ((fields & UpdatedFields.Likes) == UpdatedFields.Likes)
            {
                account.ReplaceLikes(stub.likes, false);
                SuggestIndex.Nullify(account.id);
                foreach (Like like in account.likes)
                {
                    SuggestIndex.Nullify(like.Id);
                }
            }

            UpdatePool.Return(stub);
            Index(account, fields);
        }

        private static void RemoveFromIndexes(Account account, UpdatedFields fields)
        {
            if (fields.ContainsAny(UpdatedFields.GroupIndexFlags))
            {
                GroupIndex.Remove(account);
            }

            if (fields.ContainsAny(UpdatedFields.LikesIndexFlags))
            {
                LikesIndexer.Remove(account);
            }

            if (fields.ContainsAny(UpdatedFields.City))
            {
                StringIndexer.Cities.Remove(account, account.cityIndex);
            }

            if (fields.ContainsAny(UpdatedFields.Country))
            {
                StringIndexer.Countries.Remove(account, account.countryIndex);
            }

            if (fields.ContainsAny(UpdatedFields.FName))
            {
                StringIndexer.FirstNames.Remove(account, account.fnameIndex);
            }

            if (fields.ContainsAny(UpdatedFields.SName))
            {
                StringIndexer.LastNames.Remove(account, account.snameIndex);
            }

            if (fields.ContainsAny(UpdatedFields.Email))
            {
                StringIndexer.Domains.Remove(account, account.Email._domain);
            }

            if (fields.ContainsAny(UpdatedFields.Interests))
            {
                foreach (var interest in account.InterestIndexes)
                {
                    StringIndexer.Interests.Remove(account, interest);
                }
            }

            if (fields.ContainsAny(UpdatedFields.Birth))
            {
                BirthIndex.Remove(account);
            }

            if (fields.ContainsAny(UpdatedFields.Joined))
            {
                JoinedIndex.Remove(account);
            }

            if (fields.ContainsAny(UpdatedFields.Joined | UpdatedFields.Interests))
            {
                JoinedInterestIndex.Remove(account);
            }

            if (fields.ContainsAny(UpdatedFields.RecommendIndexFlags))
            {
                RecommendIndex.Remove(account);
            }
        }

        private static void Index(Account account, UpdatedFields fields)
        {
            if (fields.ContainsAny(UpdatedFields.GroupIndexFlags))
            {
                GroupIndex.Index(account);
            }

            if (fields.ContainsAny(UpdatedFields.LikesIndexFlags))
            {
                LikesIndexer.Index(account);
            }

            if (fields.ContainsAny(UpdatedFields.City))
            {
                StringIndexer.Cities.Index(account, account.cityIndex);
            }

            if (fields.ContainsAny(UpdatedFields.Country))
            {
                StringIndexer.Countries.Index(account, account.countryIndex);
            }

            if (fields.ContainsAny(UpdatedFields.FName))
            {
                StringIndexer.FirstNames.Index(account, account.fnameIndex);
            }

            if (fields.ContainsAny(UpdatedFields.SName))
            {
                StringIndexer.LastNames.Index(account, account.snameIndex);
            }

            if (fields.ContainsAny(UpdatedFields.Email))
            {
                StringIndexer.Domains.Index(account, account.Email._domain);
            }

            if (fields.ContainsAny(UpdatedFields.Interests))
            {
                foreach (var interest in account.InterestIndexes)
                {
                    StringIndexer.Interests.Index(account, interest);
                }
            }

            if (fields.ContainsAny(UpdatedFields.Birth))
            {
                BirthIndex.Index(account);
            }

            if (fields.ContainsAny(UpdatedFields.Joined))
            {
                JoinedIndex.Index(account);
            }

            if (fields.ContainsAny(UpdatedFields.Joined | UpdatedFields.Interests))
            {
                JoinedInterestIndex.Index(account);
            }

            if (fields.ContainsAny(UpdatedFields.RecommendIndexFlags))
            {
                RecommendIndex.Index(account);
            }
        }
    }
}