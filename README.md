# MiniECS
C# minimalistic entity framework

This is a very basic entity framework intended for usage with C# 6 runtimes.

## Basic Example

```csharp
public class PositionComponent {
    public Point Point;
}

public class LinearMovementComponent
{
    public Point Direction;
    public float Step;
}

// ...
// Initialization

var ecs = new ECS(Define.EntityQuantity, 4096);

var positionComponents = ecs.CreatePool<PositionComponent>(4096);
var linearMovementComponents = ecs.CreatePool<LinearMovementComponent>(4096);

var limits = ctx.State.Simulation.SpacialLimits; // Simulation State structure defined somewhere else

for (var i = 0; i < 4096; i++) {
    var entity = ecs.CreateEntity();

    positionComponents.NewComponent(entity);
    var position = positionComponents.Get(entity);
    position.Point = random.GeneratePointWithin(limits);

    linearMovementComponents.NewComponent(entity);
    var linearMovement = linearMovementComponents.Get(entity);
    linearMovement.Direction = random.GeneratePoint().normalize();
    linearMovement.Step = random.GetFloat(0.1f, 30.0f);
}

// ...
// Update

var dt = ctx.State.Simulation.Dt;

foreach (var entity in linearMovementComponents.View()) {
    var position = ctx.Pools.PositionComponents.Get(entity);
    var linearMovement = ctx.Pools.LinearMovementComponents.Get(entity);
    var step = linearMovement.Step;

    position.Point += linearMovement.Direction * step * dt;

    if (!limits.Contain(position.Point)) {
        ecs.Destroy(entity);
    }
}
```
