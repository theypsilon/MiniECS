using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MiniECS {
    public struct BitSet {

        public const int BitfieldSize = 64;
        public const ulong One = 1;

        ulong[] _bitfield;

        public int Capacity { get { return _bitfield.Length * BitfieldSize; } }

        public void Initialize(int capacity) {
            Pre.Assert(capacity >= 0);
            var bitfieldLength = (capacity / BitfieldSize) + 1;
            _bitfield = new ulong[bitfieldLength];
            Clear();
        }

        public static BitSet New(int capacity) {
            var set = new BitSet();
            set.Initialize(capacity);
            return set;
        }

        public void GrowCapacity(int newCapacity) {
            var bitfieldLength = (newCapacity / BitfieldSize) + 1;
            var oldBitfieldLength = _bitfield.Length;
            if (bitfieldLength <= oldBitfieldLength) {
                Logger.LogWarning("BitSet.GrowCapacity not being useful!");
            }
            var oldBitfield = _bitfield;
            _bitfield = new ulong[bitfieldLength];
            for (var i = 0; i < oldBitfieldLength; i++) {
                _bitfield[i] = oldBitfield[i];
            }
            for (var i = oldBitfieldLength; i < bitfieldLength; i++) {
                _bitfield[i] = 0;
            }
        }

        public void Clear() {
            for (var i = 0; i < _bitfield.Length; i++) {
                _bitfield[i] = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int element) {
            Pre.Assert(!Contains(element), element);
            var bitfieldPos = element / BitfieldSize;
            int bitfieldOffset = element % BitfieldSize;
            ulong patch = One << bitfieldOffset;
            Pre.Assert(bitfieldPos >= 0 && bitfieldPos < _bitfield.Length, element, _bitfield.Length);
            _bitfield[bitfieldPos] += patch;
            Pre.Assert(Contains(element), element);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int element) {
            Pre.Assert(element >= 0, element);
            Pre.Assert(element < (BitfieldSize * _bitfield.Length), element, _bitfield.Length);
            var bitfieldPos = element / BitfieldSize;
            int bitfieldOffset = element % BitfieldSize;
            ulong patch = One << bitfieldOffset;
            Pre.Assert(bitfieldPos >= 0 && bitfieldPos < _bitfield.Length, element, _bitfield.Length);
            return (_bitfield[bitfieldPos] & patch) == patch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int element) {
            Pre.Assert(Contains(element), element);
            var bitfieldPos = element / BitfieldSize;
            int bitfieldOffset = element % BitfieldSize;
            ulong patch = One << bitfieldOffset;
            Pre.Assert(bitfieldPos >= 0 && bitfieldPos < _bitfield.Length, element, _bitfield.Length);
            _bitfield[bitfieldPos] -= patch;
            Pre.Assert(!Contains(element), element);
        }

        public ulong GetField(int n)
        {
            Pre.Assert(n >= 0, n);
            Pre.Assert(n < (Capacity / BitfieldSize), n);
            return _bitfield[n];
        }

        public List<int> ToNewArray() {
            var array = new List<int>();
            var count = (_bitfield.Length * BitfieldSize);
            for (var i = 0; i < count; i++) {
                if (Contains(i)) {
                    array.Add(i);
                }
            }
            return array;
        }
    }
}