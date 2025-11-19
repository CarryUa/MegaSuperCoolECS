using ECS.Components.MoveOnInput;
using ECS.Components.Transform;
using ECS.Events.InputEvents;
using ECS.System.Time;

namespace ECS.System.MoveOnInput;

public class MoveOnInputSystem : EntitySystem
{
    [SystemDependency] private readonly TimeSystem _time = default!;
    public override void Init()
    {
        base.Init();

        SubscribeEvent<MoveOnInputComponent, RightKeyPressedEvent>(OnKeyRightPressed);
        SubscribeEvent<MoveOnInputComponent, LeftKeyPressedEvent>(OnKeyLeftPressed);
        SubscribeEvent<MoveOnInputComponent, UpKeyPressedEvent>(OnKeyUpPressed);
        SubscribeEvent<MoveOnInputComponent, DownKeyPressedEvent>(OnKeyDownPressed);
    }

    public void OnKeyRightPressed(MoveOnInputComponent comp, RightKeyPressedEvent ev)
    {
        if (!TryGetComp<TransformComponent>(comp.OwnerID, out var transform)) return;

        transform!.Position.X += 0.3f * (float)_time.DeltaTime.TotalSeconds;
    }
    public void OnKeyLeftPressed(MoveOnInputComponent comp, LeftKeyPressedEvent ev)
    {
        if (!TryGetComp<TransformComponent>(comp.OwnerID, out var transform)) return;

        transform!.Position.X -= 0.3f * (float)_time.DeltaTime.TotalSeconds;
    }
    public void OnKeyUpPressed(MoveOnInputComponent comp, UpKeyPressedEvent ev)
    {
        if (!TryGetComp<TransformComponent>(comp.OwnerID, out var transform)) return;

        transform!.Position.Y += 0.3f * (float)_time.DeltaTime.TotalSeconds;
    }
    public void OnKeyDownPressed(MoveOnInputComponent comp, DownKeyPressedEvent ev)
    {
        if (!TryGetComp<TransformComponent>(comp.OwnerID, out var transform)) return;

        transform!.Position.Y -= 0.3f * (float)_time.DeltaTime.TotalSeconds;
    }
}