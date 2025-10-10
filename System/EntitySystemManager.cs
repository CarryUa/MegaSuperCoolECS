using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using ECS.Events;
using ECS.Events.AllSystemsInitializedEvent;
using ECS.Logs;
using ECS.Prototypes;

namespace ECS.System;

/// <summary>
/// Manages the initialization and updating of all EntitySystems. Also handles dependency injection and starts the loading of prototypes via <see cref="PrototypeManager.LoadPrototypes"/>.
/// </summary>
[NeedDependencies]
public class EntitySystemManager
{
    [SystemDependency] private readonly PrototypeManager _protoMan = default!;
    [SystemDependency] private readonly EventManager _evMan = default!;

    public List<EntitySystem> InitializedSystems { get => GetEntitySystems(_injectableInstances); }
    public List<object> InjectableInstances { get => _injectableInstances.ToList(); }

    /// <summary>
    /// Effectively converts ConcurrentBag<object> to List<EntitySystem> by filtering the objects.
    /// </summary>
    /// <param name="objects">The list of objects to convert.</param>
    /// <returns></returns>
    private List<EntitySystem> GetEntitySystems(ConcurrentBag<object> objects)
    {
        var returnList = new List<EntitySystem>();
        foreach (var obj in objects)
        {
            if (typeof(EntitySystem).IsAssignableFrom(obj.GetType()))
                returnList.Add((EntitySystem)obj);
        }
        return returnList;
    }

    /// <summary>
    /// List for instaces that can be injected including systems.
    /// AKA with <see cref="NeedDependencies"/> attribute
    /// </summary>
    private ConcurrentBag<object> _injectableInstances = [];


    public EntitySystemManager()
    {
        // Add self to instances
        _injectableInstances.Add(this);
    }

    /// <summary>
    /// Calls Update method on all EntitySystems.
    /// </summary>
    /// <param name="deltaT">Time elapsed since last frame.</param>
    public void UpdateAll(double deltaT)
    {
        foreach (var sys in InitializedSystems)
        {
            sys.Update(deltaT);
        }
    }

    /// <summary>
    /// Runs numerous async tasks to quickly initialize all systems and inject their dependencies.
    /// </summary>
    /// <param name="verbouse">Whether or not to log debug output.</param>
    /// <returns></returns>
    public async Task InitAllSystems(bool verbouse = false)
    {

        // Local function that initializes single object.
        void InitSystem(Type objT, bool verbouse = false)
        {
            // Create and store instance
            if (_injectableInstances.Any(obj => obj.GetType() == objT)) return;
            var sys = Activator.CreateInstance(objT);
            _injectableInstances.Add(sys!);

            // Log
            if (verbouse)
                Logger.LogDebug($"Initializing system {sys!.GetType()} : {sys!.GetHashCode()}", true, ConsoleColor.DarkBlue);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Get all classes needing dependency injectiion (Including systems)
        var objectTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<NeedDependencies>() is not null);

        // Create Task List
        var tasks = new List<Task>();

        // Fill the list with Init tasks
        foreach (var objT in objectTypes)
        {
            // Don't add it for initialization if it's not an EntitySystem
            tasks.Add(Task.Run(() => InitSystem(objT, verbouse)));
        }

        // Wait for initialization
        await Task.WhenAll(tasks);
        tasks = []; // empty the list;

        // Fill the list with Inject tasks
        foreach (var obj in _injectableInstances)
        {
            tasks.Add(InjectDependencies(obj, verbouse));
        }

        // Wait for injection
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        Logger.LogInfo($"Initialized {InitializedSystems.Count} systems in {stopwatch.ElapsedMilliseconds}ms", true, ConsoleColor.Green);

        stopwatch.Restart();

        // Load prototypes
        _protoMan.LoadPrototypes();
        stopwatch.Stop();
        Logger.LogInfo($"Loaded {_protoMan.Prototypes.Count} prototypes in {stopwatch.ElapsedMilliseconds}ms", true, ConsoleColor.Green);

        // Call the init method
        foreach (var sys in InitializedSystems.ToList())
        {
            sys.Init();
        }

        // Raise an event
        var ev = new AllSystemsInitializedEvent();
        _evMan.RaiseEvent(ev);
    }


    /// <summary>
    /// Injects all dependencies into the given object.
    /// </summary>
    /// <param name="obj">The object needing dependencies.</param>
    /// <param name="v"></param>
    /// <returns>Task representing injection of all dependencies.</returns>
    public async Task InjectDependencies(object obj, bool v = false)
    {
        // Local function to inject single dependency
        void InjectDependency(FieldInfo field, object obj, bool v = false)
        {
            if (field.GetCustomAttribute<SystemDependency>() is not null)
            {
                foreach (var _class in _injectableInstances)
                {
                    if (_class.GetType() == field.FieldType)
                    {
                        // Search for existing instance
                        var instance = _injectableInstances.First(obj => obj.GetType() == _class.GetType());


                        // Create new if doesn't exist
                        if (instance is null)
                        {
                            instance = Activator.CreateInstance(_class.GetType());
                            if (v)
                                Console.WriteLine($"Creating instance of {_class.GetType()} : {instance!.GetHashCode()}");
                            _injectableInstances.Add(instance!);
                        }
                        field.SetValue(obj, instance);
                        if (v)
                            Logger.LogDebug($"Injecting dependency {instance!.GetType()} : {instance!.GetHashCode()} into {obj}.{field.Name}", true, ConsoleColor.DarkBlue);
                        break;
                    }
                }
            }
        }


        var tasks = new List<Task>();


        var fields = obj!.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            if (obj is null) continue;
            tasks.Add(Task.Run(() => InjectDependency(field, obj, v)));
        }
        await Task.WhenAll(tasks);

        return;
    }

}

#region Attribute Def

/// <summary>
/// Marks field as dependency to be injected during initializing.        
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SystemDependency : Attribute
{
}

/// <summary>
/// Used for Systems and other classes that require dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class NeedDependencies : Attribute
{
}
#endregion