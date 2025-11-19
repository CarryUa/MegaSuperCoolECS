using System.Diagnostics;
using System.Reflection;
using ECS.Events;
using ECS.Events.SystemEvents;
using ECS.Logs;
using ECS.Prototypes;
using MyOpenTKWindow;

namespace ECS.System;

public enum InitPriority : byte
{
    Low,
    Medium,
    High
}

/// <summary>
/// Manages the initialization and updating of all EntitySystems. Also handles dependency injection and starts the loading of prototypes via <see cref="PrototypeManager.LoadPrototypes"/>.
/// </summary>
[NeedDependencies]
public partial class EntitySystemManager
{
    [SystemDependency] private readonly PrototypeManager _protoMan = default!;
    [SystemDependency] private readonly EventManager _evMan = default!;

    public List<EntitySystem> InitializedSystems { get => _injectableInstances.OfType<EntitySystem>().ToList(); }



    public EntitySystemManager(MyWindow window)
    {
        // Add self to instances
        this._injectableInstances.Add(this);
        this._injectableInstances.Add(window);
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
    public void InitAllSystems(bool verbouse = false)
    {
        void InitSystem(Type objT, bool verbouse = false)
        {
            // Create and store instance
            if (_injectableInstances.Any(obj => obj.GetType() == objT)) return;
            var sys = Activator.CreateInstance(objT);
            _injectableInstances.Add(sys!);

            if (verbouse)
                Logger.LogDebug($"Initializing system {sys!.GetType()} : {sys!.GetHashCode()}", true, ConsoleColor.DarkBlue);
        }

        // Shared stopwatch for prototypes and DI
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Get all classes needing dependency injectiion (Including systems)
        var objectTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<NeedDependencies>() is not null);

        var tasks = new List<Task>();


        // Initialize Systems
        foreach (var objT in objectTypes)
        {
            tasks.Add(Task.Run(() => InitSystem(objT, verbouse)));
        }

        Task.WhenAll(tasks).GetAwaiter().GetResult();
        tasks = []; // empty the list;

        // Fill the list with Inject tasks
        foreach (var obj in _injectableInstances)
        {
            tasks.Add(this.InjectDependencies(obj, verbouse));
        }

        // Wait for injection
        Task.WhenAll(tasks).GetAwaiter().GetResult();
        stopwatch.Stop();
        Logger.LogInfo($"Initialized {InitializedSystems.Count} systems in {stopwatch.ElapsedMilliseconds}ms", true, ConsoleColor.Green);

        stopwatch.Restart();

        // Load prototypes
        _protoMan.LoadPrototypes(verbouse);
        stopwatch.Stop();
        Logger.LogInfo($"Loaded {_protoMan.Prototypes.Count} prototypes in {stopwatch.ElapsedMilliseconds}ms", true, ConsoleColor.Green);

        // Sort systems by priority attribute
        List<EntitySystem> initList = InitializedSystems;
        initList.Sort(new EntitySystemComparer());

        // Call the init method
        foreach (var sys in initList.ToList())
        {
            sys.Init();
        }

        // Raise an event
        var ev = new AllSystemsInitializedEvent();
        _evMan.RaiseEvent(ev);
    }

}