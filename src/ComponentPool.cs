using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
//using ComponentSet = System.Collections.Generic.HashSet<MiniECS.IRemoveComponent>;
using ComponentSet = System.Collections.Generic.List<MiniECS.IRemoveComponent>;

namespace MiniECS
{
    public sealed class ComponentPool<TComponent> : IReset, IRemoveComponent
    {
        public LightArray<Entity> _Entities;
        public EntityLinker _IndexByEntity;
        public LightArray<TComponent> _Components;
        private readonly List<ComponentSet> _removeEntityShorcut;
        private BitSet _bitSet;

        private readonly bool _isReferenceType;
        private Func<TComponent> _factory;

        public ComponentPool(int capacity, EntityLinker indexArray, List<ComponentSet> removeEntityShorcut, Func<TComponent> factory)
        {
            _factory = factory;
            _bitSet.Initialize(indexArray.Array.Data.Length);
            _Entities.Initialize(capacity);
            _IndexByEntity = indexArray;
            _removeEntityShorcut = removeEntityShorcut;
            _isReferenceType = !typeof(TComponent).IsValueType;
            _Components.Initialize(capacity);
            if (_isReferenceType) {
                FillDefaultComponents(0, capacity);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity)
        {
            return entity.Id < _IndexByEntity.Array.Data.Length && _bitSet.Contains((int) entity.Id);
        }
        
        public void NewComponent(Entity entity)
        {
            CheckContains(entity);
            _NewComponentWithoutUpdatingShortcut(entity);
            _removeEntityShorcut[(int) entity.Id].Add(this);
        }

        public void _NewComponentWithoutUpdatingShortcut(Entity entity) {
            AddEntity(entity);
            var entitiesLength = _Entities.Data.Length;
            if (entitiesLength != _Components.Data.Length) {
                Pre.Assert(entitiesLength > _Components.Data.Length);
                GrowComponentCapacity(entitiesLength);
            }
            _Components.Count++;
        }

        [System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
        public void CheckContains(Entity entity) {
            var pools = _removeEntityShorcut[(int) entity.Id];
            if (pools.Contains(this)) {
                throw new Exception(typeof(TComponent) + " is already in shortcut list for entity: " + entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isEmpty() {
            return _Components.Count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LightArray<Entity> View()
        {
            return _Entities;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LightArray<TComponent> ViewComponents()
        {
            return _Components;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TComponent Get(Entity entity)
        {
            var index = _IndexByEntity.Array.Data[(int) entity.Id];
            Pre.Assert(index >= 0, index, entity.Id, entity._GetDebugName());
            Pre.Assert(index < _Components.Count, entity._GetDebugName());
            Pre.Assert(index < _Components.Data.Length, entity._GetDebugName());
            return _Components.Data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(Entity entity) {
            return _IndexByEntity.Array.Data[(int) entity.Id];
        }

        public void _RemoveComponentWithoutUpdatingShortcut(Entity entity) {
            Pre.Assert(_Components.Count > 0);

            _Components.Count--;
            var position = GetIndex(entity);
            RemoveEntity(entity);

            if (position == _Components.Count) return;

            if (_isReferenceType)
            {
                var component = _Components.Data[position];
                _Components.Data[position] = _Components.Data[_Components.Count];
                _Components.Data[_Components.Count] = component;
            }
            else
            {
                _Components.Data[position] = _Components.Data[_Components.Count];
            }
        }

        public void RemoveComponent(Entity entity)
        {
            _RemoveComponentWithoutUpdatingShortcut(entity);
            var id = (int) entity.Id;
            var shorcuts = _removeEntityShorcut[id];
            var last = shorcuts.Count - 1;
            shorcuts.Remove(this);
        }

        public int Count { get {
            return _Components.Count;
        }}

        public int Capacity { get {
            return _Components.Data.Length; 
        }}

        public void ReserveCapacity(int desiredCapacity)
        {
            var capacity = _Components.Data.Length;
            if (capacity == desiredCapacity) return;
            if (desiredCapacity > capacity) {
                GrowCapacity(desiredCapacity);
                GrowComponentCapacity(desiredCapacity);
            } else {
                _Components.ReduceCapacity(desiredCapacity);
                ReduceCapacity(desiredCapacity);
            }
        }

        public void TrimCapacity()
        {
            var desiredCapacity = _Components.Count;
            _Components.ReduceCapacity(desiredCapacity);
            ReduceCapacity(desiredCapacity);
        }

        public void Reset()
        {
            _Components.Count = 0;
            ResetEntities();
        }

        private void GrowComponentCapacity(int desiredCapacity)
        {
            Pre.Assert(desiredCapacity == _Entities.Data.Length, "This must be in sync with _Entities.Data.Length");
            var oldComponentCapacity = _Components.Data.Length;
            _Components.IncreaseCapacity(desiredCapacity);
            if (_isReferenceType) {
                FillDefaultComponents(oldComponentCapacity, _Components.Data.Length);
            }
        }

        private void FillDefaultComponents(int begin, int end) {
            for (var i = begin; i < end; i++)
            {
                _Components.Data[i] = _factory();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasEntity(Entity entity)
        {
            return entity.Id < _IndexByEntity.Array.Data.Length && _bitSet.Contains((int) entity.Id);
        }

        public void AddEntity(Entity entity)
        {            
            var id = (int) entity.Id;
            if (id >= _IndexByEntity.Array.Data.Length)
            {
                Pre.Assert(_IndexByEntity.Array.Data.Length > 0);
                var newIndexCapacity = (id / _IndexByEntity.Array.Data.Length + 1) * _IndexByEntity.Array.Data.Length;
                _IndexByEntity._GrowIndexesCapacity(newIndexCapacity);
                _bitSet.GrowCapacity(newIndexCapacity);
            }

            var position = _Entities.Count;
            if (position >= _Entities.Data.Length)
            {
                Pre.Assert(_Entities.Data.Length > 0);
                GrowEntitiesCapacity((position / _Entities.Data.Length + 1) * _Entities.Data.Length);
            }

            Pre.Assert(!HasEntity(entity), "Can't be valid: " + entity.Id);
            _IndexByEntity.Array.Data[id] = _Entities.Count;
            _Entities.Data[position] = entity;
            _Entities.Count++;
            _bitSet.Add(id);
        }

        public void RemoveEntity(Entity entity)
        {
            Pre.Assert(_Entities.Count > 0);

            _Entities.Count--;
            var id = (int) entity.Id;
            var position = _IndexByEntity.Array.Data[id];
            _IndexByEntity.Array.Data[id] = -1;
            _bitSet.Remove(id);
            
            if (position != _Entities.Count)
            {
                var lastEntity = _Entities.Data[_Entities.Count];
                _Entities.Data[position] = _Entities.Data[_Entities.Count];
                _IndexByEntity.Array.Data[(int) lastEntity.Id] = position;
            }
        }

        public void ResetEntities() {
            _Entities.Count = 0;
            _IndexByEntity.ResetIndexes();
            _bitSet.Clear();
        }
        public void ReduceCapacity(int targetCapacity)
        {
            _Entities.ReduceCapacity(targetCapacity);
        }

        public void GrowCapacity(int desiredCapacity) {
            if (desiredCapacity > _IndexByEntity.Array.Data.Length) {
                _IndexByEntity._GrowIndexesCapacity(desiredCapacity);
                _bitSet.GrowCapacity(desiredCapacity);
            }
            GrowEntitiesCapacity(desiredCapacity);
        }

        private void GrowEntitiesCapacity(int desiredCapacity)
        {
            if (desiredCapacity < _Entities.Data.Length) {
                Logger.LogWarning("Desired capacity must be equal or bigger than current capacity. desired:" + desiredCapacity + ", actual: " + _Entities.Data.Length);
                return;
            }

            if (desiredCapacity == _Entities.Data.Length) return;

            _Entities.IncreaseCapacity(desiredCapacity);
        }

        public ComponentPair[] GetDump()
        {
            if (isEmpty()) return null;
            
            var result = new ComponentPair[Count];
            var i = 0;
            foreach (var entity in _Entities) {
                result[i] = new ComponentPair() {
                    Entity = entity,
                    Component = Get(entity)
                };
                i++;
            }
            return result;
        }
    }

    public sealed class ComponentPair {
        public Entity Entity;
        public object Component;
    }

    public static class ComponentPoolCacheRecord {
        public static int Hit = 0;
        public static int Miss = 0;
    }
}