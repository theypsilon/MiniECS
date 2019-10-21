using System;

namespace MiniECS
{
    public sealed class EntityLinker {

        public EntityLinker(int capacity) {
            Array.Initialize(capacity);
            FillDefaultEntityIndexes(0, capacity);
        }
        public LightArray<int> Array;

        private void FillDefaultEntityIndexes(int begin, int end) {
            for (var i = begin; i < end; i++)
            {
                Array.Data[i] = -1;
            }
        }

        public void _GrowIndexesCapacity(int desiredCapacity)
        {
            Pre.Assert(desiredCapacity >= Array.Data.Length);
            
            if (desiredCapacity == Array.Data.Length) return;

            var oldCapacityIndexes = Array.Data.Length;
            Array.IncreaseCapacity(desiredCapacity);

            FillDefaultEntityIndexes(oldCapacityIndexes, Array.Data.Length);
        }

        public void ResetIndexes() {
            FillDefaultEntityIndexes(0, Array.Data.Length);
        }
    }

}