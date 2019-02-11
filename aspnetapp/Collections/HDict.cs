// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace aspnetapp.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    public class HDict<TKey, TValue, TComparer> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        where TComparer : class, IEqualityComparer<TKey>
    {
        protected readonly TComparer _comparer;
        protected int[] _buckets;
        protected int _count;
        protected Entry[] _entries;
        protected int _freeCount;
        protected int _freeList;
        private KeyCollection _keys;
        private ValueCollection _values;

        public HDict(int capacity, TComparer comparer)
        {
            if (capacity > 0)
            {
                Initialize(capacity);
            }


            _comparer = comparer;


            //if (typeof(TKey) == typeof(string) && _comparer == null)
            //{
            // To start, move off default comparer for string which is randomised
            //    _comparer = (TComparer)NonRandomizedStringEqualityComparer.Default;
            //}
        }

        public KeyCollection Keys
        {
            get
            {
                if (_keys == null)
                {
                    _keys = new KeyCollection(this);
                }

                return _keys;
            }
        }

        public ValueCollection Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new ValueCollection(this);
                }

                return _values;
            }
        }

        public int Count => _count - _freeCount;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (_keys == null)
                {
                    _keys = new KeyCollection(this);
                }

                return _keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new ValueCollection(this);
                }

                return _values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                var i = FindEntry(key);
                if (i >= 0)
                {
                    return _entries[i].value;
                }

                throw new KeyNotFoundException(key.ToString());
            }
            set => TryInsert(key, value, InsertionBehavior.OverwriteExisting);
        }

        public void Add(TKey key, TValue value)
        {
            var modified = TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
            Debug.Assert(modified); // If there was an existing key and the Add failed, an exception will already have been thrown.
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
            => Add(keyValuePair.Key, keyValuePair.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            var i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].value, keyValuePair.Value))
            {
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            var i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }

            return false;
        }

        public void Clear()
        {
            var count = _count;
            if (count > 0)
            {
                Array.Clear(_buckets, 0, _buckets.Length);

                _count = 0;
                _freeList = -1;
                _freeCount = 0;
                Array.Clear(_entries, 0, count);
            }
        }

        public bool ContainsKey(TKey key)
            => FindEntry(key) >= 0;

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => new Enumerator(this, Enumerator.KeyValuePair);

        // The overload Remove(TKey key, out TValue value) is a copy of this method with one additional
        // statement to copy the value for entry being removed into the output parameter.
        // Code has been intentionally duplicated for performance reasons.
        public bool Remove(TKey key)
        {
            int[] buckets = _buckets;
            Entry[] entries = _entries;
            var collisionCount = 0;

            var hashCode = (_comparer?.GetHashCode(key) ?? key.GetHashCode()) & 0x7FFFFFFF;
            var bucket = hashCode % buckets.Length;
            var last = -1;
            // Value in buckets is 1-based
            var i = buckets[bucket] - 1;
            while (i >= 0)
            {
                ref Entry entry = ref entries[i];

                if (entry.hashCode == hashCode && (_comparer?.Equals(entry.key, key) ?? EqualityComparer<TKey>.Default.Equals(entry.key, key)))
                {
                    if (last < 0)
                    {
                        // Value in buckets is 1-based
                        buckets[bucket] = entry.next + 1;
                    }
                    else
                    {
                        entries[last].next = entry.next;
                    }

                    entry.hashCode = -1;
                    entry.next = _freeList;

                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                    {
                        entry.key = default;
                    }

                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                    {
                        entry.value = default;
                    }

                    _freeList = i;
                    _freeCount++;
                    return true;
                }

                last = i;
                i = entry.next;
                if (collisionCount >= entries.Length)
                {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("Concurrent operations are not supported");
                }

                collisionCount++;
            }


            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var i = FindEntry(key);
            if (i >= 0)
            {
                value = _entries[i].value;
                return true;
            }

            value = default;
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
            => CopyTo(array, index);

        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(this, Enumerator.KeyValuePair);

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (_keys == null)
                {
                    _keys = new KeyCollection(this);
                }

                return _keys;
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new ValueCollection(this);
                }

                return _values;
            }
        }


        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            var count = _count;
            Entry[] entries = _entries;
            for (var i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                }
            }
        }

        public Enumerator GetEnumerator()
            => new Enumerator(this, Enumerator.KeyValuePair);


        private int FindEntry(TKey key)
        {
            var i = -1;
            int[] buckets = _buckets;
            Entry[] entries = _entries;
            var collisionCount = 0;

            TComparer comparer = _comparer;

            var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            // Value in _buckets is 1-based
            i = buckets[hashCode % buckets.Length] - 1;
            do
            {
                // Should be a while loop https://github.com/dotnet/coreclr/issues/15476
                // Test in if to drop range check for following array access
                if ((uint)i >= (uint)entries.Length ||
                    entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    break;
                }

                i = entries[i].next;
                if (collisionCount >= entries.Length)
                {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("Concurrent operations are not supported");
                }

                collisionCount++;
            } while (true);


            return i;
        }

        private int Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);

            _freeList = -1;
            _buckets = new int[size];
            _entries = new Entry[size];

            return size;
        }

        private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
        {
            Entry[] entries = _entries;
            TComparer comparer = _comparer;

            var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;

            var collisionCount = 0;
            ref var bucket = ref _buckets[hashCode % _buckets.Length];
            // Value in _buckets is 1-based
            var i = bucket - 1;


            do
            {
                // Should be a while loop https://github.com/dotnet/coreclr/issues/15476
                // Test uint in if rather than loop condition to drop range check for following array access
                if ((uint)i >= (uint)entries.Length)
                {
                    break;
                }

                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if (behavior == InsertionBehavior.OverwriteExisting)
                    {
                        entries[i].value = value;
                        return true;
                    }

                    if (behavior == InsertionBehavior.ThrowOnExisting)
                    {
                        throw new ArgumentException($"Key {key} is already added to collection", nameof(key));
                    }

                    return false;
                }

                i = entries[i].next;
                if (collisionCount >= entries.Length)
                {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("Concurrent operations are not supported");
                }

                collisionCount++;
            } while (true);


            var updateFreeList = false;
            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                updateFreeList = true;
                _freeCount--;
            }
            else
            {
                var count = _count;
                if (count == entries.Length)
                {
                    Resize();
                    bucket = ref _buckets[hashCode % _buckets.Length];
                }

                index = count;
                _count = count + 1;
                entries = _entries;
            }

            ref Entry entry = ref entries[index];

            if (updateFreeList)
            {
                _freeList = entry.next;
            }

            entry.hashCode = hashCode;
            // Value in _buckets is 1-based
            entry.next = bucket - 1;
            entry.key = key;
            entry.value = value;
            // Value in _buckets is 1-based
            bucket = index + 1;


            return true;
        }


        protected void Resize()
            => Resize(HashHelpers.ExpandPrime(_count), false);

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            // Value types never rehash

            var buckets = new int[newSize];
            var entries = new Entry[newSize];

            var count = _count;
            Array.Copy(_entries, 0, entries, 0, count);

            if (default(TKey) == null && forceNewHashCodes)
            {
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        Debug.Assert(_comparer == null);
                        entries[i].hashCode = entries[i].key.GetHashCode() & 0x7FFFFFFF;
                    }
                }
            }

            for (var i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    var bucket = entries[i].hashCode % newSize;
                    // Value in _buckets is 1-based
                    entries[i].next = buckets[bucket] - 1;
                    // Value in _buckets is 1-based
                    buckets[bucket] = i + 1;
                }
            }

            _buckets = buckets;
            _entries = entries;
        }

        // This overload is a copy of the overload Remove(TKey key) with one additional
        // statement to copy the value for entry being removed into the output parameter.
        // Code has been intentionally duplicated for performance reasons.
        public bool Remove(TKey key, out TValue value)
        {
            int[] buckets = _buckets;
            Entry[] entries = _entries;
            var collisionCount = 0;

            var hashCode = (_comparer?.GetHashCode(key) ?? key.GetHashCode()) & 0x7FFFFFFF;
            var bucket = hashCode % buckets.Length;
            var last = -1;
            // Value in buckets is 1-based
            var i = buckets[bucket] - 1;
            while (i >= 0)
            {
                ref Entry entry = ref entries[i];

                if (entry.hashCode == hashCode && (_comparer?.Equals(entry.key, key) ?? EqualityComparer<TKey>.Default.Equals(entry.key, key)))
                {
                    if (last < 0)
                    {
                        // Value in buckets is 1-based
                        buckets[bucket] = entry.next + 1;
                    }
                    else
                    {
                        entries[last].next = entry.next;
                    }

                    value = entry.value;

                    entry.hashCode = -1;
                    entry.next = _freeList;

                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                    {
                        entry.key = default;
                    }

                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                    {
                        entry.value = default;
                    }

                    _freeList = i;
                    _freeCount++;
                    return true;
                }

                last = i;
                i = entry.next;
                if (collisionCount >= entries.Length)
                {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("Concurrent operations are not supported");
                }

                collisionCount++;
            }

            value = default;
            return false;
        }

        public bool TryAdd(TKey key, TValue value)
            => TryInsert(key, value, InsertionBehavior.None);

        /// <summary>
        /// Ensures that the dictionary can hold up to 'capacity' entries without any further expansion of its backing storage
        /// </summary>
        private int EnsureCapacity(int capacity)
        {
            var currentCapacity = _entries == null ? 0 : _entries.Length;
            if (currentCapacity >= capacity)
            {
                return currentCapacity;
            }

            var newSize = HashHelpers.GetPrime(capacity);
            Resize(newSize, false);
            return newSize;
        }

        /// <summary>
        /// Sets the capacity of this dictionary to what it would be if it had been originally initialized with all its entries
        /// This method can be used to minimize the memory overhead
        /// once it is known that no new elements will be added.
        /// To allocate minimum size storage array, execute the following statements:
        /// dictionary.Clear();
        /// dictionary.TrimExcess();
        /// </summary>
        public void TrimExcess()
            => TrimExcess(Count);

        /// <summary>
        /// Sets the capacity of this dictionary to hold up 'capacity' entries without any further expansion of its backing storage
        /// This method can be used to minimize the memory overhead
        /// once it is known that no new elements will be added.
        /// </summary>
        private void TrimExcess(int capacity)
        {
            var newSize = HashHelpers.GetPrime(capacity);

            Entry[] oldEntries = _entries;
            var currentCapacity = oldEntries == null ? 0 : oldEntries.Length;
            if (newSize >= currentCapacity)
            {
                return;
            }

            var oldCount = _count;
            Initialize(newSize);
            Entry[] entries = _entries;
            int[] buckets = _buckets;
            var count = 0;
            for (var i = 0; i < oldCount; i++)
            {
                var hashCode = oldEntries[i].hashCode;
                if (hashCode >= 0)
                {
                    ref Entry entry = ref entries[count];
                    entry = oldEntries[i];
                    var bucket = hashCode % newSize;
                    // Value in _buckets is 1-based
                    entry.next = buckets[bucket] - 1;
                    // Value in _buckets is 1-based
                    buckets[bucket] = count + 1;
                    count++;
                }
            }

            _count = count;
            _freeCount = 0;
        }

        private enum InsertionBehavior : byte
        {
            /// <summary>
            /// The default insertion behavior.
            /// </summary>
            None = 0,

            /// <summary>
            /// Specifies that an existing entry with the same key should be overwritten if encountered.
            /// </summary>
            OverwriteExisting = 1,

            /// <summary>
            /// Specifies that if an existing entry with the same key is encountered, an exception should be thrown.
            /// </summary>
            ThrowOnExisting = 2
        }

        protected struct Entry
        {
            public int hashCode; // Lower 31 bits of hash code, -1 if unused
            public int next; // Index of next entry, -1 if last
            public TKey key; // Key of entry
            public TValue value; // Value of entry
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
            IDictionaryEnumerator
        {
            private readonly HDict<TKey, TValue, TComparer> _dictionary;
            private int _index;
            private readonly int _getEnumeratorRetType; // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(HDict<TKey, TValue, TComparer> dictionary, int getEnumeratorRetType)
            {
                _dictionary = dictionary;
                _index = 0;
                _getEnumeratorRetType = getEnumeratorRetType;
                Current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                //if (_version != _dictionary._version)
                //{
                //    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                //}

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is int.MaxValue
                while ((uint)_index < (uint)_dictionary._count)
                {
                    ref Entry entry = ref _dictionary._entries[_index++];

                    if (entry.hashCode >= 0)
                    {
                        Current = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
                        return true;
                    }
                }

                _index = _dictionary._count + 1;
                Current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current { get; private set; }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    //if (_index == 0 || _index == _dictionary._count + 1)
                    //{
                    //    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                    //}

                    if (_getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(Current.Key, Current.Value);
                    }

                    return new KeyValuePair<TKey, TValue>(Current.Key, Current.Value);
                }
            }

            void IEnumerator.Reset()
            {
                //if (_version != _dictionary._version)
                //{
                //    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                //}

                _index = 0;
                Current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Current.Key, Current.Value);

            object IDictionaryEnumerator.Key => Current.Key;

            object IDictionaryEnumerator.Value => Current.Value;
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            private readonly HDict<TKey, TValue, TComparer> _dictionary;

            public KeyCollection(HDict<TKey, TValue, TComparer> dictionary)
            {
                _dictionary = dictionary;
            }

            void ICollection.CopyTo(Array array, int index) => throw new NotSupportedException();

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

            public void CopyTo(TKey[] array, int index)
            {
                var count = _dictionary._count;
                Entry[] entries = _dictionary._entries;
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        array[index++] = entries[i].key;
                    }
                }
            }

            public int Count => _dictionary.Count;

            bool ICollection<TKey>.IsReadOnly => true;

            void ICollection<TKey>.Add(TKey item)
                => throw new NotSupportedException();

            void ICollection<TKey>.Clear()
                => throw new NotSupportedException();

            bool ICollection<TKey>.Contains(TKey item)
                => _dictionary.ContainsKey(item);

            bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
                => new Enumerator(_dictionary);

            IEnumerator IEnumerable.GetEnumerator()
                => new Enumerator(_dictionary);

            public Enumerator GetEnumerator()
                => new Enumerator(_dictionary);

            public struct Enumerator : IEnumerator<TKey>, IEnumerator
            {
                private readonly HDict<TKey, TValue, TComparer> _dictionary;
                private int _index;

                internal Enumerator(HDict<TKey, TValue, TComparer> dictionary)
                {
                    _dictionary = dictionary;
                    _index = 0;
                    Current = default;
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    //if (_version != _dictionary._version)
                    //{
                    //    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    //}

                    while ((uint)_index < (uint)_dictionary._count)
                    {
                        ref Entry entry = ref _dictionary._entries[_index++];

                        if (entry.hashCode >= 0)
                        {
                            Current = entry.key;
                            return true;
                        }
                    }

                    _index = _dictionary._count + 1;
                    Current = default;
                    return false;
                }

                public TKey Current { get; private set; }

                object IEnumerator.Current => Current;

                void IEnumerator.Reset()
                {
                    // if (_version != _dictionary._version)
                    // {
                    // ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    //  }

                    _index = 0;
                    Current = default;
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection : ICollection<TValue>, IReadOnlyCollection<TValue>
        {
            private readonly HDict<TKey, TValue, TComparer> _dictionary;

            public ValueCollection(HDict<TKey, TValue, TComparer> dictionary)
            {
                _dictionary = dictionary;
            }

            public void CopyTo(TValue[] array, int index)
            {
                var count = _dictionary._count;
                Entry[] entries = _dictionary._entries;
                for (var i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        array[index++] = entries[i].value;
                    }
                }
            }

            public int Count => _dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            void ICollection<TValue>.Add(TValue item)
                => throw new NotSupportedException();

            bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();

            void ICollection<TValue>.Clear()
                => throw new NotSupportedException();

            bool ICollection<TValue>.Contains(TValue item)
                => throw new NotSupportedException();

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
                => new Enumerator(_dictionary);

            IEnumerator IEnumerable.GetEnumerator()
                => new Enumerator(_dictionary);

            public Enumerator GetEnumerator()
                => new Enumerator(_dictionary);

            public struct Enumerator : IEnumerator<TValue>, IEnumerator
            {
                private readonly HDict<TKey, TValue, TComparer> _dictionary;
                private int _index;

                internal Enumerator(HDict<TKey, TValue, TComparer> dictionary)
                {
                    _dictionary = dictionary;
                    _index = 0;
                    Current = default;
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    //if (_version != _dictionary._version)
                    //{
                    //   ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    //}

                    while ((uint)_index < (uint)_dictionary._count)
                    {
                        ref Entry entry = ref _dictionary._entries[_index++];

                        if (entry.hashCode >= 0)
                        {
                            Current = entry.value;
                            return true;
                        }
                    }

                    _index = _dictionary._count + 1;
                    Current = default;
                    return false;
                }

                public TValue Current { get; private set; }

                object IEnumerator.Current => Current;

                void IEnumerator.Reset()
                {
                    //if (_version != _dictionary._version)
                    //{
                    //    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
                    //}

                    _index = 0;
                    Current = default;
                }
            }
        }
    }
}