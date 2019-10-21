using System.Collections;
using System.Collections.Generic;

namespace MiniECS
{
    public struct LightArray<T>
    {
        public T[] Data;
        public int Count;
        public T _Current;
        public int _Index;

        public void Initialize(int capacity = 4) {
            Pre.Assert(Data == null);
            Data = new T[capacity];
        }

        public void Dispose()
        {}

        public bool MoveNext()
        {
            if (_Index >= Count)
            {
                return false;
            }
            _Current = Data[_Index];
            _Index++;
            return true;
        }

        public T Current
        {
            get
            {
                Pre.Assert(_Index > 0 && _Index < Count + 1);
                return _Current;
            }
        }

        public int Capacity
        {
            get
            {
                return Data.Length;
            }
        }

        public void IncreaseCapacity(int desiredCapacity) {
            Pre.Assert(Data != null);
            var oldCapacity = Data.Length;
            Pre.Assert(desiredCapacity > oldCapacity, "Use ReduceCapacity, to reduce capacity.");

            DebugLogStupidAllocation(oldCapacity, desiredCapacity);
            
            var oldArray = Data;
            Data = new T[desiredCapacity];
            for (var i = 0; i < oldCapacity; i++)
            {
                Data[i] = oldArray[i];
            }
        }

        public void ReduceCapacity(int targetCapacity) {
            Pre.Assert(Data != null);
            Pre.Assert(targetCapacity >= Count, "Try using Reset first if you want to free all memory.");
            var oldCapacity = Data.Length;
            Pre.Assert(targetCapacity <= oldCapacity, "Use IncreaseCapacity to increase capacity.");
            if (targetCapacity == oldCapacity) return;

            DebugLogStupidAllocation(oldCapacity, targetCapacity);

            var oldArray = Data;
            Data = new T[targetCapacity];
            for (var i = 0; i < targetCapacity; i++)
            {
                Data[i] = oldArray[i];
            }
        }

        public LightArray<T> GetEnumerator()
        {
            _Index = 0;
            _Current = default(T);
            return this;
        }

	    [System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
        private void DebugLogStupidAllocation(int oldLength, int newLength) {
            if ((oldLength > newLength && oldLength / 2 < newLength) || (newLength > oldLength && newLength < oldLength * 2)) {
                Logger.LogWarning("Stupid allocation going from " + oldLength + " to " + newLength);
            }
        }

        public T[] ToNewArray() {
            var array = new T[Count];
            for (var i = 0; i < Count; i++) {
                array[i] = Data[i];
            }
            return array;
        }
    }
}