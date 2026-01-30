using ECS.Components.Transform;
using ECS.Entities;
using ECS.Prototypes;
using ECS.System.Time;

#pragma warning disable CS0649

namespace ECS.System;

public class TestSystem : EntitySystem
{
    [SystemDependency] private readonly EntityManager _ent = default!;

    public override void Init()
    {
        base.Init();
        _ent.CreateEntity("ThisIsFine");
    }
}