using System.Reflection;
using ECS.Components;
using ECS.Entities;
using ECS.Events;
using ECS.Logs;

namespace ECS.System;

[NeedDependencies]
[InitializationPriority(InitPriority.Low)]
public class EntitySystem : IComparable<EntitySystem>
{

    [SystemDependency]
    protected readonly EventManager _evMan = default!;

    [SystemDependency] private readonly EntityManager _entMan = default!;
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

    public override string ToString()
    {
        return $"{this.GetType()} : {this.GetHashCode()}";
    }

    /// <summary>
    /// Calls <see cref="Logger.LogInfo(object?, bool, ConsoleColor)"/> and adds the name of this system in the beggining of message
    /// </summary>
    public void LogInfo(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        msg = $"{this.GetType().Name}: " + msg;
        Logger.LogInfo(msg, fancy, color);
    }

    /// <summary>
    /// Calls <see cref="Logger.LogError(object?, bool, ConsoleColor)"/> and adds the name of this system in the beggining of message
    /// </summary>
    public void LogError(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        msg = $"{this.GetType().Name}: " + msg;
        Logger.LogError(msg, fancy, color);
    }

    /// <summary>
    /// Calls <see cref="Logger.LogDebug(object?, bool, ConsoleColor)"/> and adds the name of this system in the beggining of message
    /// </summary>
    public void LogDebug(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        msg = $"{this.GetType().Name}: " + msg;

        Logger.LogDebug(msg, fancy, color);
    }

    public bool HasComp<TComp>(int id)
    where TComp : IComponent
    {
        var ent = _entMan.GetEntityById(id);
        if (ent is null) return false;
        if (ent.Components.Any(c => c.GetType() == typeof(TComp))) return true;
        return false;
    }
    public bool HasComp<TComp>(IEntity ent)
    where TComp : IComponent
    {
        if (ent is null) return false;
        if (ent.Components.Any(c => c.GetType() == typeof(TComp))) return true;
        return false;
    }

    public bool TryGetComp<TComp>(int id, out TComp? component)
    where TComp : IComponent
    {
        component = default;
        var ent = _entMan.GetEntityById(id);
        if (ent is null) return false;

        component = (TComp?)ent.Components.FirstOrDefault(c => c.GetType() == typeof(TComp));
        return component is not null;
    }

    public bool TryGetComp<TComp>(IEntity ent, out TComp? component)
    where TComp : IComponent
    {
        component = default;
        if (ent is null) return false;

        component = (TComp?)ent.Components.FirstOrDefault(c => c.GetType() == typeof(TComp));
        return component is not null;
    }

    /// <inheritdoc/>
    /// <exception cref="NullReferenceException">
    /// </exception>
    public int CompareTo(EntitySystem? other)
    {
        if (other is null)
        {
            throw new NullReferenceException($"Tried to compare {this} to unititialized {other}.");
        }

        var otherAttrib = other.GetType().GetCustomAttribute<InitializationPriority>();
        var myAttrib = this.GetType().GetCustomAttribute<InitializationPriority>();

        // If both attributes are null - they're equal.
        if (myAttrib is null && otherAttrib is null) return 0;

        // If only other' attribute is null - my is better.
        if (otherAttrib is null) return -1;

        // If only my attribute is null - my is worse.
        if (myAttrib is null) return 1;

        // Compare priorities.
        if (myAttrib.Priority > otherAttrib.Priority)
            return -1;
        else if (myAttrib.Priority < otherAttrib.Priority)
            return 1;

        // Equal priority.
        return 0;
    }
}