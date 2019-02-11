using System;
using aspnetapp.Controllers;

namespace aspnetapp.Collections
{
    public class HGroupDict : HDict<GroupKey, int, GroupKey.GroupKeyEqualityComparer>
    {
        public HGroupDict(int capacity) : base(capacity, GroupKey.Comparer)
        {
        }
        
        /// <summary>
        /// This method is specially for group method
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool AddOrUpdate(GroupKey key, int value)
        {
            Entry[] entries = _entries;
            GroupKey.GroupKeyEqualityComparer comparer = _comparer;

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
                    entries[i].value += value;
                    return true;
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
    }
}