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

    /// <summary>
    /// Subsctibes all components of given type to given event type.
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <typeparam name="TEv"></typeparam>
    /// <param name="action">The callback function that will be invoked when the event is raised.</param>
    /// <exception cref="InvalidCastException"></exception>
    protected void SubscribeEvent<TComp, TEv>(Action<TComp, TEv> action)
    where TComp : IComponent
    where TEv : IEvent
    {
        _evMan.SubscribeEvent(action);
    }

    /// <summary>
    /// Raises the given event by invoking all callbacks.
    /// </summary>
    /// <typeparam name="TEv"></typeparam>
    /// <param name="ev"></param>
    protected void RaiseEvent(Event ev)
    {
        _evMan.RaiseEvent(ev);
    }
}