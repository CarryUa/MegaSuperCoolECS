using System.Diagnostics;
using System.Reflection;
using ECS.Components;
using ECS.Events;
using ECS.Logs;

namespace ECS.Systems;

[InjectableDependency]
public class EntitySystem
{
    [SystemDependency] protected EventManager _evMan = default!;
    [SystemDependency] protected CompManager _compMan = default!;


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
    /// Called once when all dependencies of this were injected.
    /// </summary>
    public virtual void Init()
    {
    }

    public void SubscribeEvent<TComp, TEv>(Action<TComp, TEv> action)
    where TComp : Component
    where TEv : Event
    {
        var _event = new Event();
        var act = new Action<Component, Event>((comp, ev) =>
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


        // Check if same component is subscribed already
        if (_evMan.ActiveSubsctiptions.Where(ev => ev.Key.GetType() == typeof(TComp)).Count() > 0)
        {
            foreach (var sub in _evMan.ActiveSubsctiptions)
            {
                if (sub.Key.GetType() == typeof(TComp))
                {
                    sub.Value.Actions.Add(act);
                    return;
                }
            }
        }

        _event.Actions.Add(act);

        foreach (var comp in _compMan.Components.Where(c => c is TComp))
        {
            Logger.LogInfo($"Adding ({comp}, {_event}) Event to EventManager");
            _evMan.ActiveSubsctiptions.Add(comp, _event);
        }

        Logger.LogInfo(_event.Actions);
    }
    public void RaiseEvent(Event ev)
    {
        _evMan.RaiseEvent(ev);
    }
}



public class EntSysManager
{
    public List<EntitySystem> InitializedSystems = [];

    public void UpdateAll(double deltaT)
    {
        foreach (var sys in InitializedSystems)
        {
            sys.Update(deltaT);
        }
    }

    public async Task InitAllSystems(bool verbouse = false)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var EntSysType = typeof(EntitySystem);

        // Get all children of EntitySystem
        var entSyss = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass
                                                                            && EntSysType.IsAssignableFrom(t));

        // Create Task List
        var tasks = new List<Task>();

        // Fill the list with Init tasks
        foreach (var sysT in entSyss)
        {
            tasks.Add(Task.Run(() => InitSystem(sysT, verbouse)));
        }

        // Wait for initialization
        await Task.WhenAll(tasks);
        tasks = []; // empty the list;

        // Fill the list with Inject tasks
        foreach (var sys in InitializedSystems.ToList())
        {
            tasks.Add(InjectDependencies(sys, verbouse));
        }
        // Wait for injection
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        Logger.LogInfo($"Initialized {InitializedSystems.Count} systems in {stopwatch.ElapsedMilliseconds}ms", true, ConsoleColor.Green);

        // Call the init method
        foreach (var sys in InitializedSystems.ToList())
        {
            sys.Init();
        }

    }

    public void InitSystem(Type system, bool verbouse = false)
    {
        var sys = (EntitySystem?)Activator.CreateInstance(system);
        InitializedSystems.Add(sys!);
        if (verbouse)
            Logger.LogInfo($"Initializing system {sys!.GetType()} : {sys!.GetHashCode()}", true, ConsoleColor.DarkBlue);
    }

    public async Task InjectDependencies(EntitySystem system, bool v = false)
    {
        var tasks = new List<Task>();

        var allInjectableDependency = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<InjectableDependency>() != null && typeof(EntitySystem).IsAssignableFrom(t));

        var fields = system!.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (system is null) continue;
            tasks.Add(Task.Run(() => InjectDependency(field, system, allInjectableDependency, v)));
        }
        await Task.WhenAll(tasks);

        return;
    }

    public void InjectDependency(FieldInfo field, EntitySystem system, IEnumerable<Type> allInjectableDependency, bool v = false)
    {
        if (field.GetCustomAttribute<SystemDependency>() is not null)
        {
            foreach (var _classT in allInjectableDependency)
            {
                if (_classT == field.FieldType)
                {
                    // Search for existing instance
                    var instance = InitializedSystems.Find(sys => sys is not null && sys.GetType() == _classT);

                    // Create new if doesn't exist
                    if (instance is null)
                    {
                        instance = (EntitySystem?)Activator.CreateInstance(_classT);
                        if (v)
                            Console.WriteLine($"Creating instance of {_classT} : {instance!.GetHashCode()}");
                        InitializedSystems.Add(instance!);
                    }
                    field.SetValue(system, instance);
                    if (v)
                        Logger.LogInfo($"Injecting dependency {instance!.GetType()} : {instance!.GetHashCode()} into {system}.{field.Name}", true, ConsoleColor.DarkBlue);
                    break;
                }
            }
        }
    }
}

#region Attribute Def

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SystemDependency : Attribute
{
}


[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class InjectableDependency : Attribute
{
}


#endregion