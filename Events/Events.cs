using ECS.Components;
using ECS.System;
namespace ECS.Events;

public interface IEvent
{
    bool Handled { get; set; }
    Action<IComponent, IEvent> Action { get; set; }
}

public class Event : IEvent
{
    public bool Handled { get; set; }
    public Action<IComponent, IEvent> Action { get; set; } = (comp, ev) => { }; // Empty action
}

[NeedDependencies]
public sealed class EventManager
{
    [SystemDependency] private readonly CompManager _compMan = default!;
    private Queue<KeyValuePair<IComponent, IEvent>> ActiveSubscriptions = new();

    public void SubscribeEvent<TComp, TEv>(Action<TComp, TEv> action)
    where TComp : IComponent
    where TEv : IEvent
    {
        var _event = Activator.CreateInstance<TEv>();

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
                    throw new Exception("Wrong Event Type Exception");
            });
        _event.Action += act;

        // Iterate through all components of type TComp and subscribe them to this event.
        foreach (var comp in _compMan.Components.Where(c => c is TComp))
        {
            // Fill the queue with component-event pairs
            // KeyValuePair<Component, IEvent>
            ActiveSubscriptions.Enqueue(new(comp, _event));
        }
    }

    public void RaiseEvent<TEv>(TEv ev)
    where TEv : IEvent
    {
        foreach (var subscription in ActiveSubscriptions)
        {
            if (subscription.Value.GetType() == ev.GetType())
                subscription.Value.Action.Invoke(subscription.Key, ev);
        }
    }
}