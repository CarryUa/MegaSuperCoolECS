using ECS.Components;

namespace ECS.Events.ComponentEvents;

/// <summary>
/// Raised when <see cref="ComponentManager.CloneComponent(IComponent)"/> or <see cref="ComponentManager.CloneComponent{TComp}(TComp)"/> is called.
/// </summary>
public class ComponentCreatedEvent : Event
{
    public IComponent Component { get; set; }

    public ComponentCreatedEvent(IComponent component)
    {
        Component = component;
    }
}