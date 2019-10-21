using System;
using System.Collections.Generic;

namespace MiniECS
{
    public struct Entity : IEquatable<Entity>
    {
        public readonly uint Id;

        public Entity(uint id)
        {
            Id = id;
        }

        public static bool operator ==(Entity lhs, Entity rhs)
        {
            return lhs.Id == rhs.Id;
        }
            
        public static bool operator !=(Entity lhs, Entity rhs)
        {
            return lhs.Id != rhs.Id;
        }

        public bool Equals(Entity item)
        {
            return item.Id == Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        [System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
        public void _SetDebugName(string debugName)
        {
            EntityDebug.SetEntityName(this, debugName);
        }

        [System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
        public void _SetDebugParent(ECS ecs)
        {
            EntityDebug.SetEntityParent(this, ecs);
        }
            
        public string _GetDebugName()
        {
            return EntityDebug.GetEntityName(this);
        }
        public List<IRemoveComponent> _GetDebugComponents() {
            return EntityDebug.GetEntityParent(this)._GetComponentsByEntity(this);
        }

        [System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
        public void _PrintDebugComponents() {
            var strings = _GetDebugName() + "-" + Id + "-components:\n";
            foreach (var name in _GetDebugComponents()) {
                strings += name + "\n";
            }
            Logger.Log(strings + ".\n");
        }

#if DEBUG
        public override string ToString() {
            return "Entity" + Id + "." + _GetDebugName();
        }
#endif
    }

    public static class EntityDebug
    {
        private static readonly Dictionary<Entity, string> DebugNames = new Dictionary<Entity, string>();
        private static readonly Dictionary<Entity, ECS> DebugParents = new Dictionary<Entity, ECS>();

        public static void SetEntityName(Entity entity, string name)
        {
            if (DebugNames.ContainsKey(entity))
            {
                DebugNames[entity] = name;
            }
            else
            {
                DebugNames.Add(entity, name);   
            }
        }

        public static void SetEntityParent(Entity entity, ECS ecs) {
            if (DebugParents.ContainsKey(entity))
            {
                DebugParents[entity] = ecs;
            }
            else
            {
                DebugParents.Add(entity, ecs);   
            }
        }

        public static ECS GetEntityParent(Entity entity) {
            return DebugParents.ContainsKey(entity) ? DebugParents[entity] : null;   
        }

        public static string GetEntityName(Entity entity)
        {
            return DebugNames.ContainsKey(entity) ? DebugNames[entity] : "";
        }
    }
}