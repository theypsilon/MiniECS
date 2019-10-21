using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MiniECS {

    public sealed class Set<T> : ICollection<T> {
        HashSet<T> _set;
        private int _capacity;

        public Set(int capacity) {
            _set = new HashSet<T>();
            _capacity = capacity;
        }

        public int Capacity {
            get {
                return _capacity > _set.Count ? _capacity : _set.Count;
            }
        }
        public int Count {
            get {
                return _set.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            _set.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _set.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            return _set.Remove(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _set.GetEnumerator();
        }
    }

}