using ECS.Components;
using ECS.System;
namespace ECS.Events;

public interface IEvent
{
    bool Handled { get; set; }
}

/// <summary>
/// 
/// </summary>
/// <param name="tcomp"></param>
/// <param name="tev"></param>
public class EventSubscription(Type tcomp, Type tev, Action<IComponent, IEvent> action)
{
    public Type ComponentType { get; } = tcomp;
    public Type EventType { get; } = tev;
    public Action<IComponent, IEvent> Action { get; } = action;
}

/// <summary>
/// Basic event. All events should inherit from it.
/// </summary>
public class Event : IEvent
{
    public bool Handled { get; set; }
}

/// <summary>
/// Handles the subsctiption and rising of the events.
/// </summary>
[NeedDependencies]
public sealed class EventManager
{
    [SystemDependency] private readonly ComponentManager _compMan = default!;
    // <IComponent, Type>, action = <comp, TEv>, action
    private Queue<EventSubscription> ActiveSubscriptions = new();

    /// <summary>
    /// Subsctibes all components of given type to given event type.
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <typeparam name="TEv"></typeparam>
    /// <param name="action">The callback function that will be invoked when the event is raised.</param>
    /// <exception cref="InvalidCastException"></exception>
    public void SubscribeEvent<TComp, TEv>(Action<TComp, TEv> action)
    where TComp : IComponent
    where TEv : IEvent
    {
        // Wrap the action in additional type checks.
        var act = new Action<IComponent, IEvent>((comp, ev) =>
            {
                if (comp is TComp tComp && ev is TEv tEv)
                {
                    // Additional type check
                    if (typeof(TComp) != tComp.GetType()) return;

                    action(tComp, tEv);
                }
                else
                    throw new InvalidCastException("Wrong Event Type Exception");
            });

        // Fill the queue with (component-event)-action pairs
        // KeyValuePair<(Component, IEvent), action>
        ActiveSubscriptions.Enqueue(new(typeof(TComp), typeof(TEv), act));
    }

    /// <summary>
    /// Raises the given event by invoking all callbacks.
    /// </summary>
    /// <param name="ev"></param>
    public void RaiseEvent(IEvent ev)
    {
        foreach (var subscription in ActiveSubscriptions.ToList())
        {
            if (subscription.EventType == ev.GetType())
            {
                foreach (var comp in _compMan.Components.Where(c => c.GetType() == subscription.ComponentType).ToList())
                {

                    subscription.Action(comp, ev);
                }
            }
        }
    }
}