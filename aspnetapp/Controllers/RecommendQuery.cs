using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using aspnetapp.Collections;
using aspnetapp.Domain;
using aspnetapp.Extensions;

namespace aspnetapp.Controllers
{
    public class RecommendQuery :
        IComparer<(Account account, Compatibility compatibility)>,
        IComparer<KeyValuePair<uint, Compatibility>>
    {
        public int limit;
        private ushort _countryIndex;
        private ushort _cityIndex;
        public bool WillYieldZeroResults;
        private readonly HList<KeyValuePair<uint, Compatibility>> _compatibilities;
        private readonly HList<Account> _results;
        private readonly RecMultiUnionList _union;

        public RecommendQuery()
        {
            _compatibilities = new HList<KeyValuePair<uint, Compatibility>>(1000);
            _results = new HList<Account>(60);
            _union = new RecMultiUnionList();
            Reset();
        }

        public void Reset()
        {
            _cityIndex = ushort.MaxValue;
            _countryIndex = ushort.MaxValue;
            WillYieldZeroResults = false;
            _compatibilities.Clear();
            _results.Clear();
            _union.Clear();
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

        public HList<Account> Execute(Account account)
        {
            Filter(account);

            if (_compatibilities.Count == 0)
            {
                return AbstractQuery.Empty;
            }

            if (_compatibilities.Count < limit)
            {
                _compatibilities.Sort(this);
            }

            for (uint i = 0; i < _compatibilities.Count; i++)
            {
                _results.Add(Database.GetAccount(_compatibilities[i].Key));
            }

            return _results;
        }

        private void Filter(Account account)
        {

            RecommendIndex.GetUnionForRecommendQuery(account, _union, _cityIndex, _countryIndex);
            _union.Prepare();
            while (_union.MoveNext(out int sharedInterests))
            {
                var candidate = Database.GetAccount(_union.Current.Id);
                var compatibility = Compatibility.Of(account, candidate, sharedInterests, _union.Current);

                //Console.WriteLine($"{candidate.id}: {compatibility}");
                if (!Add(candidate.id, compatibility))
                {
                    break;
                }
            }
        }
        public int Compare((Account account, Compatibility compatibility) x, (Account account, Compatibility compatibility) y)
        {
            //sort by DESCENDING compatibility and ASCENDING id
            var result = y.compatibility.CompareTo(x.compatibility);
            return result == 0
                ? x.account.id.CompareTo(y.account.id)
                : result;
        }

        public int Compare(KeyValuePair<uint, Compatibility> x, KeyValuePair<uint, Compatibility> y)
        {
            var result = y.Value.CompareTo(x.Value);
            return result == 0
                ? x.Key.CompareTo(y.Key)
                : result;
        }

        private bool Add(uint cid, Compatibility compatibility)
        {
            if (compatibility > 0) //TODO: got rid of the check
            {
                var item = new KeyValuePair<uint, Compatibility>(cid, compatibility);
                if (_compatibilities.Count < (limit-1))
                {
                    _compatibilities.Add(item);
                    return true;
                }
                if (_compatibilities.Count == limit - 1)
                {
                    _compatibilities.Add(item);
                    _compatibilities.Sort(this);
                    return true;
                }

                var last = _compatibilities[(uint) (_compatibilities.Count-1)];

                if (last.Value.HasPremium && !compatibility.HasPremium)
                {
                    return false;
                }

                if (compatibility.Status < last.Value.Status)
                {
                    return false;
                }

                if (Compare(last, item) > 0)
                {
                    _compatibilities.RemoveAt(_compatibilities.Count - 1);
                    var binarySearch = _compatibilities.BinarySearch(0, _compatibilities.Count, item, this);
                    var insertAt = ~binarySearch;
                    _compatibilities.Insert(insertAt, item);
                }

                return true;
            }

            return true;
        }
    }
}