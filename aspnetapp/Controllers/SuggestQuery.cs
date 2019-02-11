using System;
using System.Collections.Generic;
using System.Linq;
using aspnetapp.Collections;
using aspnetapp.Domain;

namespace aspnetapp.Controllers
{
    public class SuggestQuery : IComparer<KeyValuePair<uint, Similarity>>
    {
        private ushort _cityIndex;
        private ushort _countryIndex;
        public int limit;

        public bool WillYieldZeroResults;
        private readonly HashSet<uint> _resultSet;
        private readonly HList<KeyValuePair<uint, Similarity>> _list;
        private readonly HList<Account> _result;

        public void Reset()
        {
            _cityIndex = ushort.MaxValue; //means not set, zero means city is not found in index
            _countryIndex = ushort.MaxValue; //means not set, zero means country is not found in index
            WillYieldZeroResults = false;
            _resultSet.Clear();
            _result.Clear();
            _list.Clear();
        }

        public SuggestQuery()
        {
            _resultSet = new HashSet<uint>(100);
            _list = new HList<KeyValuePair<uint, Similarity>>();
            _result = new HList<Account>();
            Reset();
        }



        public bool AddCountry(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            WillYieldZeroResults = WillYieldZeroResults || !StringIndexer.Countries.TryGetIndex(value, out _countryIndex);
            return true;
        }

        public bool AddCity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            WillYieldZeroResults = WillYieldZeroResults || !StringIndexer.Cities.TryGetIndex(value, out _cityIndex);
            return true;
        }

        public HList<Account> Execute(Account me)
        {
            if (me.likes == null || _cityIndex == 0 || _countryIndex == 0)
            {
                return HList<Account>.Empty;
            }

            if (_cityIndex == ushort.MaxValue && _countryIndex == ushort.MaxValue && SuggestIndex.TryGetIndex(me.id, out HList<uint> indexed))
            {
                var maxItems = Math.Min(limit, indexed.Count);

                for (uint i = 0; i < maxItems; i++)
                {
                    _result.Add(Database.GetAccount(indexed[i]));
                }

                return _result;
            }

            return ExecuteWithoutIndex(me);
        }

        public HList<Account> ExecuteWithoutIndex(Account me)
        {
            if (me.likes == null || _cityIndex == 0 || _countryIndex == 0)
            {
                return HList<Account>.Empty;
            }

            //TODO: city or country index highly likely to work better cause of smaller final record set
            foreach (Like likedByMe in me.likes)
            {
                HList<uint> candidates = LikesIndexer.GetAllWhoLikes(likedByMe.Id);
                _resultSet.UnionWith(candidates);
            }

            _resultSet.Remove(me.id);

            Span<uint> accounts = stackalloc uint[150];
            if (_cityIndex < ushort.MaxValue)
            {
                foreach (uint cid in _resultSet)
                {
                    Account candidate = Database.GetAccount(cid);
                    if (_cityIndex != candidate.cityIndex)
                    {
                        continue;
                    }

                    CalcSimilarity(me, candidate, ref accounts);
                }
            }
            else if (_countryIndex < ushort.MaxValue)
            {
                foreach (uint cid in _resultSet)
                {
                    Account candidate = Database.GetAccount(cid);
                    if (_countryIndex != candidate.countryIndex)
                    {
                        continue;
                    }

                    CalcSimilarity(me, candidate, ref accounts);
                }
            }
            else
            {
                foreach (uint cid in _resultSet)
                {
                    Account candidate = Database.GetAccount(cid);
                    CalcSimilarity(me, candidate, ref accounts);
                }
            }

            var maxItems = Math.Min(limit, _list.Count);

            for (uint i = 0; i < maxItems; i++)
            {
                _result.Add(Database.GetAccount(_list[i].Key));
            }

            return _result;
        }

        private void CalcSimilarity(Account me, Account candidate, ref Span<uint> accounts)
        {
            Similarity similarity = Similarity.Of(me, candidate, accounts, out uint count);

            if (count == 0)
            {
                return;
            }

            if (limit <= _list.Count && similarity < _list[(uint) (_list.Count - 1)].Value)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var item = new KeyValuePair<uint, Similarity>(accounts[i], similarity);
                if (_list.Count < (limit - 1))
                {
                    _list.Add(item);
                    continue;
                }

                if (_list.Count == limit - 1)
                {
                    _list.Add(item);
                    _list.Sort(this);
                    continue;
                }


                var index = _list.BinarySearch(item, this);
                if (index > 0)
                {
                    continue;
                }

                var listIndex = (uint) (_list.Count - 1);
                var last = _list[listIndex];
                if (Compare(last, item) > 0)
                {
                    _list.RemoveAt((int) listIndex);
                    var binarySearch = _list.BinarySearch(0, _list.Count, item, this);
                    var insertAt = ~binarySearch;
                    _list.Insert(insertAt, item);
                }
            }
        }

        public int Compare(KeyValuePair<uint, Similarity> x, KeyValuePair<uint, Similarity> y)
        {
            var compareTo = y.Value.CompareTo(x.Value);
            return compareTo != 0 ? compareTo : y.Key.CompareTo(x.Key);
        }
    }
}