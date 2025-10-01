using System.Diagnostics;
using ECS.Components;
using ECS.Systems;

namespace ECS.Events;

public class Event()
{
    public bool Handled { get; set; }
    public List<Action<Component, Event>> Actions { get; set; } = []; // Empty action
}

public sealed class EventManager : EntitySystem
{
    [DebuggerDisplay("EventSubscription")] public Dictionary<Component, Event> ActiveSubsctiptions = [];
    public new void RaiseEvent(Event ev)
    {
        foreach (var subscription in ActiveSubsctiptions)
        {
            if (subscription.Value.GetType() == ev.GetType())
                foreach (var action in subscription.Value.Actions)
                    action.Invoke(subscription.Key, subscription.Value);
        }
    }
}