using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MiniECS
{

    public interface IRemoveComponent  {
        void _RemoveComponentWithoutUpdatingShortcut(Entity entity);
        ComponentPair[] GetDump();
    }

    public interface IReset {
        void Reset();
    }

}