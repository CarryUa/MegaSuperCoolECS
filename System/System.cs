using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using ECS.Components;
using ECS.Events;
using ECS.Events.AllSystemsInitializedEvent;
using ECS.Logs;

namespace ECS.System;

[InjectableDependency]
public class EntitySystem
{
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

    protected void SubscribeEvent<TComp, TEv>(Action<TComp, TEv> action)
    where TComp : Component
    where TEv : IEvent
    {
        EventManager.SubscribeEvent(action);
    }
    protected void RaiseEvent(Event ev)
    {
        EventManager.RaiseEvent(ev);
    }
}



public class EntSysManager
{
    public ConcurrentBag<EntitySystem> InitializedSystems = [];

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

        var ev = new AllSystemsInitializedEvent();
        EventManager.RaiseEvent(ev);
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
                    var instance = InitializedSystems.First(sys => sys is not null && sys.GetType() == _classT);

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