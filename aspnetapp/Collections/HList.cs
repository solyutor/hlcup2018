// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace aspnetapp.Collections
{


    // Implements a variable-size List that uses an array of objects to store the
    // elements. A List has a capacity, which is the allocated length
    // of the internal array. As elements are added to a List, the capacity
    // of the List is automatically increased as required by reallocating the
    // internal array.
    //
    [DebuggerDisplay("Count = {Count}")]
    public class HList<T> : IList<T>, IReadOnlyList<T>
    {
        public class DescendingComparer<T> : IComparer<T>
        {
            public int Compare(T x, T y) => Comparer<T>.Default.Compare(y, x);
        }

        public static readonly DescendingComparer<T> Descending = new DescendingComparer<T>();

        private const int DefaultCapacity = 4;

        private T[] _items; // Do not rename (binary serialization)
        private int _size; // Do not rename (binary serialization)

        private static readonly T[] s_emptyArray = new T[0];
        public static HList<T> Empty = new HList<T>();

        // Constructs a List. The list is initially empty and has a capacity
        // of zero. Upon adding the first element to the list the capacity is
        // increased to DefaultCapacity, and then increased in multiples of two
        // as required.

        public T[] Items => _items;
        public HList()
        {
            _items = s_emptyArray;
        }

        // Constructs a List with a given initial capacity. The list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required.
        //
        public HList(int capacity)
        {
            if (capacity == 0)
                _items = s_emptyArray;
            else
                _items = new T[capacity];
        }

        public HList(ICollection<T> collection)
        {
            int count = collection.Count;
            if (count == 0)
            {
                this._items = HList<T>.s_emptyArray;
            }
            else
            {
                this._items = new T[count];
                collection.CopyTo(this._items, 0);
                this._size = count;
            }
        }

        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value < _size)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (_size > 0)
                        {
                            Array.Copy(_items, 0, newItems, 0, _size);
                        }
                        _items = newItems;
                    }
                    else
                    {
                        _items = s_emptyArray;
                    }
                }
            }
        }

        // Read-only property describing how many elements are in the List.
        public int Count => _size;
        public bool IsReadOnly => false;

        // Sets or Gets the element at the given index.
        public T this[uint index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        // Adds the given object to the end of this list. The size of the list is
        // increased by one. If required, the capacity of the list is doubled
        // before adding the new element.
        //

        public void Add(T item) => AddReturnIndex(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddReturnIndex(T item)
        {
            T[] array = _items;
            int size = _size;
            if ((uint)size < (uint)array.Length)
            {
                _size = size + 1;
                array[size] = item;
            }
            else
            {
                AddWithResize(item);
            }
            return _size;
        }

        // Non-inline from List.Add to improve its code quality as uncommon path
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddWithResize(T item)
        {
            int size = _size;
            EnsureCapacity(size + 1);
            _size = size + 1;
            _items[size] = item;

        }


        public bool InsertAscending(T value)
        {
            return InsertFast(value, Comparer<T>.Default);
        }

        public bool InsertDescending(T value)
        {
            return InsertFast(value, Descending);
        }

        private bool InsertFast(T value, IComparer<T> comparer)
        {
            var indexToInsert = BinarySearch(0, Count, value, comparer);
            if (indexToInsert >= 0)
            {
                //Already added to list;
                return false;
            }

            indexToInsert = ~indexToInsert;

            if (indexToInsert == Count)
            {
                Add(value);
            }
            else
            {
                Insert(indexToInsert, value);
            }

            return true;
        }


        public bool RemoveDescending(T value)
        {
            var index = BinarySearchDescending(value);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public bool RemoveAscending(T value)
        {
            var index = BinarySearch(value, Comparer<T>.Default);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }


        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return Array.BinarySearch<T>(_items, index, count, item, comparer);
        }

        public int BinarySearchDescending(T item)
            => BinarySearch(0, Count, item, Descending);

        public int BinarySearchDescending(T item, int start, int count)
            => BinarySearch(start, count, item, Descending);

        public int BinarySearchAscending(T item)
            => BinarySearch(0, Count, item, null);

        public int BinarySearch(T item, IComparer<T> comparer)
            => BinarySearch(0, Count, item, comparer);

        // Clears the contents of List.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                int size = _size;
                _size = 0;
                if (size > 0)
                {
                    Array.Clear(_items, 0, size); // Clear the elements so that the gc can reclaim the references.
                }
            }
            else
            {
                _size = 0;
            }
        }

        public void ClearFast() => _size = 0;

        // Contains returns true if the specified element is in the List.
        // It does a linear, O(n) search.  Equality is determined by calling
        // EqualityComparer<T>.Default.Equals().
        //
        public bool Contains(T item)
        {
            // PERF: IndexOf calls Array.IndexOf, which internally
            // calls EqualityComparer<T>.Default.IndexOf, which
            // is specialized for different types. This
            // boosts performance since instead of making a
            // virtual method call each iteration of the loop,
            // via EqualityComparer<T>.Default.Equals, we
            // only make one virtual call to EqualityComparer.IndexOf.

            return _size != 0 && IndexOf(item) != -1;
        }

        // Copies this List into array, which must be of a
        // compatible array type.
        public void CopyTo(T[] array)
            => CopyTo(array, 0);


        // Copies a section of this list to the given array at the given index.
        //
        // The method uses the Array.Copy method to copy the elements.
        //
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            // Delegate rest of error checking to Array.Copy.
            Array.Copy(_items, index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            // Delegate rest of error checking to Array.Copy.
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        // Ensures that the capacity of this list is at least the given minimum
        // value. If the current capacity of the list is less than min, the
        // capacity is increased to twice the current capacity or to min,
        // whichever is larger.
        //
        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
                // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if ((uint)newCapacity > int.MaxValue) newCapacity = int.MaxValue;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        public bool Exists(Predicate<T> match)
            => FindIndex(match) != -1;

        public T Find(Predicate<T> match)
        {
            for (int i = 0; i < _size; i++)
            {
                if (match(_items[i]))
                {
                    return _items[i];
                }
            }
            return default;
        }

        public HList<T> FindAll(Predicate<T> match)
        {
            HList<T> list = new HList<T>();
            for (int i = 0; i < _size; i++)
            {
                if (match(_items[i]))
                {
                    list.Add(_items[i]);
                }
            }
            return list;
        }

        public int FindIndex(Predicate<T> match)
            => FindIndex(0, _size, match);

        public int FindIndex(int startIndex, Predicate<T> match)
            => FindIndex(startIndex, _size - startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(_items[i])) return i;
            }
            return -1;
        }

        public T FindLast(Predicate<T> match)
        {
            for (int i = _size - 1; i >= 0; i--)
            {
                if (match(_items[i]))
                {
                    return _items[i];
                }
            }
            return default;
        }

        public int FindLastIndex(Predicate<T> match)
            => FindLastIndex(_size - 1, _size, match);

        public int FindLastIndex(int startIndex, Predicate<T> match)
            => FindLastIndex(startIndex, startIndex + 1, match);

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            int endIndex = startIndex - count;
            for (int i = startIndex; i > endIndex; i--)
            {
                if (match(_items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < _size; i++)
            {
                action(_items[i]);
            }
        }

        // Returns an enumerator for this list with the given
        // permission for removal of elements. If modifications made to the list
        // while an enumeration is in progress, the MoveNext and
        // GetObject methods of the enumerator will throw an exception.
        //
        public Enumerator GetEnumerator()
            => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(this);

        public HList<T> GetRange(int index, int count)
        {
            HList<T> list = new HList<T>(count);
            Array.Copy(_items, index, list._items, 0, count);
            list._size = count;
            return list;
        }

        public void SortDescending()
        {
            Array.Sort(this._items, 0, this._size, Descending);
        }


        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards from beginning to end.
        // The elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public int IndexOf(T item) => Array.IndexOf(_items, item, 0, _size);



        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards, starting at index
        // index and ending at count number of elements. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public int IndexOf(T item, int index)
        {
            return Array.IndexOf(_items, item, index, _size - index);
        }

        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards, starting at index
        // index and upto count number of elements. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public int IndexOf(T item, int index, int count)
        {
            return Array.IndexOf(_items, item, index, count);
        }

        // Inserts an element into this list at a given index. The size of the list
        // is increased by one. If required, the capacity of the list is doubled
        // before inserting the new element.
        //
        public void Insert(int index, T item)
        {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Array.Copy(_items, index, _items, index + 1, _size - index);
            }
            _items[index] = item;
            _size++;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(this._size, collection);
        }

        // Inserts the elements of the given collection at a given index. If
        // required, the capacity of the list is increased to twice the previous
        // capacity or the new size, whichever is larger.  Ranges may be added
        // to the end of the list by setting index to the List's size.
        //
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection is ICollection<T> c)
            {
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(_size + count);
                    if (index < _size)
                    {
                        Array.Copy(_items, index, _items, index + count, _size - index);
                    }

                    // If we're inserting a List into itself, we want to be able to deal with that.
                    if (this == c)
                    {
                        // Copy first part of _items to insert location
                        Array.Copy(_items, 0, _items, index, index);
                        // Copy last part of _items back to inserted location
                        Array.Copy(_items, index + count, _items, index * 2, _size - index);
                    }
                    else
                    {
                        c.CopyTo(_items, index);
                    }
                    _size += count;
                }
            }
            else
            {
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Insert(index++, en.Current);
                    }
                }
            }
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at the end
        // and ending at the first element in the list. The elements of the list
        // are compared to the given value using the Object.Equals method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public int LastIndexOf(T item)
        {
            if (_size == 0)
            {  // Special case for empty list
                return -1;
            }
            else
            {
                return LastIndexOf(item, _size - 1, _size);
            }
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at index
        // index and ending at the first element in the list. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, index + 1);
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at index
        // index and upto count elements. The elements of
        // the list are compared to the given value using the Object.Equals
        // method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public int LastIndexOf(T item, int index, int count)
        {
            if (_size == 0)
            {  // Special case for empty list
                return -1;
            }

            return Array.LastIndexOf(_items, item, index, count);
        }

        // Removes the element at the given index. The size of the list is
        // decreased by one.
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }
        // This method removes all items which matches the predicate.
        // The complexity is O(n).
        public int RemoveAll(Predicate<T> match)
        {
            int freeIndex = 0;   // the first free slot in items array

            // Find the first item which needs to be removed.
            while (freeIndex < _size && !match(_items[freeIndex])) freeIndex++;
            if (freeIndex >= _size) return 0;

            int current = freeIndex + 1;
            while (current < _size)
            {
                // Find the first item which needs to be kept.
                while (current < _size && match(_items[current])) current++;

                if (current < _size)
                {
                    // copy item to the free slot.
                    _items[freeIndex++] = _items[current++];
                }
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_items, freeIndex, _size - freeIndex); // Clear the elements so that the gc can reclaim the references.
            }

            int result = _size - freeIndex;
            _size = freeIndex;
            return result;
        }

        // Removes the element at the given index. The size of the list is
        // decreased by one.
        public void RemoveAt(int index)
        {
            _size--;
            if (index < _size)
            {
                Array.Copy(_items, index + 1, _items, index, _size - index);
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _items[_size] = default;
            }
        }

        T IList<T>.this[int index] { get => this[(uint) index]; set => this[(uint) index] = value; }

        // Removes a range of elements from this list.
        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                _size -= count;
                if (index < _size)
                {
                    Array.Copy(_items, index + count, _items, index, _size - index);
                }

                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    Array.Clear(_items, _size, count);
                }
            }
        }

        // Reverses the elements in this list.
        public void Reverse()
            => Reverse(0, Count);

        // Reverses the elements in a range of this list. Following a call to this
        // method, an element in the range given by index and count
        // which was previously located at index i will now be located at
        // index index + (index + count - i - 1).
        //
        public void Reverse(int index, int count)
        {
            if (count > 1)
            {
                Array.Reverse(_items, index, count);
            }
        }

        // Sorts the elements in this list.  Uses the default comparer and
        // Array.Sort.
        public void Sort()
            => Sort(0, Count, null);

        // Sorts the elements in this list.  Uses Array.Sort with the
        // provided comparer.
        public void Sort(IComparer<T> comparer)
            => Sort(0, Count, comparer);

        // Sorts the elements in a section of this list. The sort compares the
        // elements to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented by all
        // elements of the list.
        //
        // This method uses the Array.Sort method to sort the elements.
        //
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (count > 1)
            {
                Array.Sort<T>(_items, index, count, comparer);
            }
        }

        // ToArray returns an array containing the contents of the List.
        // This requires copying the List, which is an O(n) operation.
        public T[] ToArray()
        {
            if (_size == 0)
            {
                return s_emptyArray;
            }

            T[] array = new T[_size];
            Array.Copy(_items, 0, array, 0, _size);
            return array;
        }

        // Sets the capacity of this list to the size of the list. This method can
        // be used to minimize a list's memory overhead once it is known that no
        // new elements will be added to the list. To completely clear a list and
        // release all memory referenced by the list, execute the following
        // statements:
        //
        // list.Clear();
        // list.TrimExcess();
        //
        public void TrimExcess()
        {
            int threshold = (int)(((double)_items.Length) * 0.9);
            if (_size < threshold)
            {
                Capacity = _size;
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {

            for (int i = 0; i < _size; i++)
            {
                if (!match(_items[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly HList<T> _list;
            private int _index;
            private T _current;

            internal Enumerator(HList<T> list)
            {
                _list = list;
                _index = 0;
                _current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                HList<T> localList = _list;

                if (((uint)_index >= (uint)localList._size))
                {
                    return MoveNextRare();
                }
                _current = localList._items[_index];
                _index++;
                return true;

            }

            public void Previous() => _index--;

            private bool MoveNextRare()
            {
                _index = _list._size + 1;
                _current = default;
                return false;
            }

            public T Current => _current;

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }

            object IEnumerator.Current => Current;
        }

        public HList<T> PrintData()
        {
            Console.WriteLine(string.Join(",", this));
            return this;
        }

        T IReadOnlyList<T>.this[int index] { get => this[(uint) index]; }

        public ref T GetByRef(int i)
        {
            return ref _items[i];
        }
    }
}
