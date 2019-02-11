using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using aspnetapp.Sys;

namespace aspnetapp
{
    //TODO: Remplace ints with shorts.
    public class StringIndexer
    {
        public const int MaxCityCount = 700;
        public const int MaxCountryCount = 80;
        public const int MaxInterestCount = 100;
        private const int MaxFirstNameCount = 120;
        private const int MaxLastNameCount = 1680;
        private const int MaxDomainCount = 20;


        private readonly bool _quote;
        private readonly bool _indexNotNull;
        private readonly bool _indexNull;
        public static readonly StringIndexer Cities = new StringIndexer(MaxCityCount);
        public static readonly StringIndexer Countries = new StringIndexer(MaxCountryCount);
        public static readonly StringIndexer Interests = new StringIndexer(MaxInterestCount, indexNotNull: false, indexNull: false);

        public static readonly StringIndexer FirstNames = new StringIndexer(MaxFirstNameCount, indexNotNull: false, indexNull:false);
        public static readonly StringIndexer LastNames = new StringIndexer(MaxLastNameCount, indexNotNull: false, indexNull:false);

        public static readonly StringIndexer Domains = new StringIndexer(MaxDomainCount, false, false, false);


        private readonly string[] _index2String;
        private readonly HOrdinalDict<ushort> _string2Index;
        private readonly Utf8String[] _index2Bytes;
        private readonly HList<uint>[] _index2Accounts;
        private readonly HDict<Utf8String, ushort, Utf8String.ContentComparer> _utf82index;

        private static readonly Utf8String NullString = UnsafeStringContainer.GetString("null", false);
        private ushort _currentIndex;
        private readonly int _notNullIndex;


        protected StringIndexer(int capacity, bool quote = true, bool indexNotNull = true, bool indexNull = true)
        {
            _quote = quote;
            _indexNotNull = indexNotNull;
            _indexNull = indexNull;
            _notNullIndex = capacity;
            _currentIndex = 0;
            _string2Index = new HOrdinalDict<ushort>(capacity);
            //zero index is for nulls
            _index2String = new string[capacity];
            _index2String[0] = null;

            _index2Bytes = new Utf8String[capacity];
            _index2Bytes[0] = NullString;

            _index2Accounts = new HList<uint>[capacity + 1];
            _index2Accounts[0] = new HList<uint>(); //for null queries
            _index2Accounts[capacity] = new HList<uint>(); //for not null queries

            _utf82index = new HDict<Utf8String, ushort, Utf8String.ContentComparer>(capacity, Utf8String.DefaultComparer);
        }

        public uint Count => _currentIndex;

        public string this[ushort index] => _index2String[index];

        public int this[string index] => _string2Index[index];

        public ushort GetOrAdd(string value)
        {
            if (value == null)
            {
                return 0;
            }

            if (_string2Index.TryGetValue(value, out ushort result))
            {
                return result;
            }


            result = ++_currentIndex;
            _index2String[result] = value;
            Utf8String utf8String = UnsafeStringContainer.GetString(value, _quote);
            _index2Bytes[result] = utf8String;
            _utf82index.Add(utf8String, result);

            _string2Index.Add(value, result);
            _index2Accounts[result] = new HList<uint>();

            return result;
        }

        public bool TryGetIndex(string value, out ushort index) => _string2Index.TryGetValue(value, out index);

        public HList<uint> GetList(uint index) => _index2Accounts[index];

        public Utf8String GetBytes(ushort index) => _index2Bytes[index];

        public void Index(Account account, ushort index)
        {
            if (_indexNull || index > 0)
            {
                _index2Accounts[index].InsertDescending(account.id);
            }

            if (_indexNotNull && index > 0)
            {
                _index2Accounts[_notNullIndex].InsertDescending(account.id);
            }
        }

        public void Remove(Account account, ushort index)
        {
            if (_indexNull || index > 0)
            {
                _index2Accounts[index].RemoveDescending(account.id);
            }

            if (_indexNotNull && index > 0)
            {
                _index2Accounts[_notNullIndex].RemoveDescending(account.id);
            }

        }

        public HList<uint>[] GetAllLists()
        {
            return _index2Accounts;
        }


        public static void TrimAll()
        {
            Cities.Trim();
            Countries.Trim();
            FirstNames.Trim();
            LastNames.Trim();
            Interests.Trim();
        }

        private void Trim()
        {
            foreach (var list in _index2Accounts)
            {
                list?.TrimExcess();
                //list?.Sort();
            }
        }

        public void PrintStats(string name)
        {
            Console.WriteLine($"Index: {name} {Count}");
            for (uint i = 0; i < Count; i++)
            {
                var origin = _index2String[i];
                int count = _index2Accounts[i]?.Count ?? 0;
                Console.WriteLine($"{origin}: {count}");
            }
            Console.WriteLine();
        }

        public void PrintCollisions(string name)
        {
            Console.WriteLine("Collisions for " + name);
            var grouping = _index2Bytes
                .Select(x => new {value = x.ToString(), hash = Utf8String.DefaultComparer.GetHashCode(x)})
                .GroupBy(x => x.hash)
                .Select(x => new {hash = x.Key, count = x.Count(), values = string.Join(",", x.Select(y => y.value))})
                .OrderByDescending(x => x.count)
                .Where(x => x.count > 1);
            foreach (var group in grouping)
            {
                Console.WriteLine($"{group.count}: {group.values}");
            }
        }

        public ushort Find(Utf8String value)
        {
            return _utf82index[value];
        }
    }
}