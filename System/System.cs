using ECS.Components;
using ECS.Events;

namespace ECS.System;

[NeedDependencies]
public class EntitySystem
{

    [SystemDependency]
    protected readonly EventManager _evMan = default!;
    // public bool Initialized
    // {
    //     get => _entMan.InitializedSystems.First(obj => obj == this) is not null;
    // }

    public EntitySystem()
    {
    }

    /// <summary>
    /// Called once per frame by Window object.
    /// </summary>
    /// <remarks>
    /// This is called before frame is rendered.
    /// </remarks>
    /// <param name="deltaT">The time elapsed since the last frame. In seconds.</param>
    public virtual void Update(double deltaT)
    {

    }

    /// <summary>
    /// Called once when all dependencies of this were injected. AKA right after initialization.
    /// </summary>
    public virtual void Init()
    {
    }

    protected void SubscribeEvent<TComp, TEv>(Action<TComp, TEv> action)
    where TComp : IComponent
    where TEv : IEvent
    {
        _evMan.SubscribeEvent(action);
    }
    protected void RaiseEvent(Event ev)
    {
        _evMan.RaiseEvent(ev);
    }
}