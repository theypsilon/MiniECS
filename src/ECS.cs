using System;
using System.Collections.Generic;
using ComponentSet = System.Collections.Generic.List<MiniECS.IRemoveComponent>;
//using ComponentSet = System.Collections.Generic.HashSet<MiniECS.IRemoveComponent>;

namespace MiniECS
{
    // TODO change on C# 7.0 to ref returns
    // ReSharper disable once InconsistentNaming
    public sealed class ECS
    {
        private readonly List<bool> _entityValidation = new List<bool>();
        private readonly Queue<Entity> _availableEntities = new Queue<Entity>();
        private readonly List<IReset> _resetters = new List<IReset>();
        private readonly List<IRemoveComponent> _pools = new List<IRemoveComponent>();
        private readonly List<ComponentSet> _removeEntitiesShortcut;

        private Entity _invalidEntity;

        public ECS(int entitiesCapacity = 128, int poolsCapacity = 64, int groupsCapacity = 32)
        {
            _entityValidation = new List<bool>(entitiesCapacity);
            _availableEntities = new Queue<Entity>(entitiesCapacity);
            _resetters = new List<IReset>(poolsCapacity + groupsCapacity);
            _removeEntitiesShortcut = new List<ComponentSet>(entitiesCapacity);
            for(var i = 0; i < entitiesCapacity; i++) {
                _removeEntitiesShortcut.Add(new ComponentSet(poolsCapacity / 4));
            }
            _invalidEntity = CreateEntity();
        }

        public Entity GetInvalidEntity()
        {
            return _invalidEntity;
        }
        
        public Entity CreateEntity()
        {
            if (_availableEntities.Count != 0)
            {
                var entity = _availableEntities.Dequeue();
                var id = (int) entity.Id;
                CheckCreation(id);
                _entityValidation[id] = true;
                entity._SetDebugName("entity");
                return entity;
            }
            else
            {
                var id = (uint)_entityValidation.Count;
                var entity = new Entity(id);
                _entityValidation.Add(true);
                if (_removeEntitiesShortcut.Count < _entityValidation.Count) {
                    _removeEntitiesShortcut.Add(new ComponentSet(_pools.Count / 4));
                }
                entity._SetDebugName("entity");
                entity._SetDebugParent(this);
                return entity;
            }
        }

        public ComponentPool<TComponent> CreatePool<TComponent>(int capacity = 32, EntityLinker indexes = null, Func<TComponent> factory = null) where TComponent : new()
        {
            var pool = new ComponentPool<TComponent>(
                capacity,
                indexes ?? new EntityLinker(_entityValidation.Capacity),
                _removeEntitiesShortcut,
                factory ?? (() => new TComponent())
            );
            _pools.Add(pool);
            _resetters.Add(pool);
            return pool;
        }

        public bool IsValid(Entity entity)
        {
            return entity != _invalidEntity && entity.Id < _entityValidation.Count && _entityValidation[(int) entity.Id];
        }

        public void Destroy(Entity entity)
        {
            Pre.Assert(IsValid(entity), entity._GetDebugName());
            
            var id = (int) entity.Id;
            CheckDestruction(id);

            foreach (var pool in _removeEntitiesShortcut[id]) {
                pool._RemoveComponentWithoutUpdatingShortcut(entity);
            }

            _entityValidation[id] = false;
            _availableEntities.Enqueue(entity);
            _removeEntitiesShortcut[id].Clear();
        }

        [System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
        public void CheckCreation(int id) {
            
            if (_entityValidation[id]) {
                throw new System.Exception("Entity id: " + id + " was already valid!");
            }
        }

        [System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
        public void CheckDestruction(int id) {
            Pre.Assert(id < _entityValidation.Count, id);
            if (!_entityValidation[id]) {
                throw new System.Exception("Entity id: " + id + " was already invalid!");
            }
        }

        public int CalculateHowManyEntities() {
            int total = 0;
            foreach (var boolean in _entityValidation) {
                if (boolean) {
                    total++;
                }
            }
            return total;
        }

        public List<bool> GetValidationList() {
            return _entityValidation;
        }

        public ComponentSet _GetComponentsByEntity(Entity entity) {
            return _removeEntitiesShortcut[(int) entity.Id];
        }

        public void Reset()
        {
            foreach (var shorcuts in _removeEntitiesShortcut) {
                shorcuts.Clear();
            }
            _entityValidation.Clear();
            _availableEntities.Clear();
            foreach (var resetter in _resetters)
            {
                resetter.Reset();
            }
            _invalidEntity = CreateEntity();
        }

        public Dictionary<string, ComponentPair[]> GetDumps() {
            var dict = new Dictionary<string, ComponentPair[]>(_pools.Count);
            for (var i = 0; i < _pools.Count; i++) {
                var pool = _pools[i];
                dict.Add(pool.GetType().ToString(), pool.GetDump());
            }
            return dict;
        }
    }
}