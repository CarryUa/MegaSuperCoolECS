using ECS.Components.Transform;
using ECS.Entities;
using ECS.System.Time;

namespace ECS.System;

public class TestSystem : EntitySystem
{
    [SystemDependency] private readonly EntityManager _entMan = default!;
    [SystemDependency] private readonly TimeSystem _time = default!;

    private TransformComponent? c;


    public override void Init()
    {
        base.Init();
        var ent = _entMan.CreateEntity("PlayerPrototype");
        ent = _entMan.CreateEntity("SpriteTestProto");

        TryGetComp<TransformComponent>(ent, out var comp);
        comp!.Position.X += 0.7f;
        c = comp;
        LogInfo("Created test entity1");

    }

    public override void Update(double deltaT)
    {
        base.Update(deltaT);

        c!.Position.X = MathF.Sin((float)_time.Time.TotalSeconds);
    }
}