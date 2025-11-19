

using ECS.Components;

namespace ECS.Events.ComponentEvents;

/// <summary>
/// Raised when <see cref="ComponentManager.CloneComponent(IComponent)"/> or <see cref="ComponentManager.CloneComponent{TComp}(TComp)"/> is called.
/// </summary>
public class ComponentClonedEvent(IComponent newC, IComponent originalC) : ComponentCreatedEvent(newC)
{
    public IComponent OriginalComp { get; set; } = originalC;
}