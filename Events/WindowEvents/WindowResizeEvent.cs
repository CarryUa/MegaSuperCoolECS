using OpenTK.Mathematics;

namespace ECS.Events.WindowEvents;

public class WindowResizeEvent : Event
{
    public Vector2i OldSize;
    public Vector2i NewSize;
    public float OldAspect;
    public float NewAspect;
}