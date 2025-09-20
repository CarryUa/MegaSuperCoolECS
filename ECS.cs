using System.Drawing;
using System.Reflection;
using Events;
namespace ECS;

public class Entity
{
    public Entity(List<Component> components)
    {
        Components = components;
    }

    public List<Component> Components { get; set; } = [];

};

public class Component(int newId)
{
    public int id
    {
        get => _id;
    }

    protected int _id = newId;

};

public class CompManager() : EntitySystem
{
    public List<Component> Components { get => _components; }
    private List<Component> _components = [];

    public Component CreateComponent<TComp>() where TComp : Component
    {
        var newId = _components.Count;

        var comp = Activator.CreateInstance(typeof(TComp), newId);
        _components.Add((Component)comp!);
        Logger.LogInfo($"Creating new component of type {comp!.GetType()} : {comp.GetHashCode()} with Component.id {newId}", true, ConsoleColor.DarkBlue);

        return (Component)comp!;
    }
}

[InjectableDependency]
public class EntitySystem
{
    [SystemDependency] protected EventManager _evMan = default!;
    [SystemDependency] protected CompManager _compMan = default!;

    public EntitySystem()
    {
    }

    public virtual void Update(double deltaT)
    {

    }
    public virtual void Init()
    {
        _compMan.CreateComponent<HelloWorldComponent>();
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

    public void InitAllSystems(bool verbouse = false)
    {
        var EntSysType = typeof(EntitySystem);

        // Get all children of EntitySystem
        var entSyss = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass
                                                                            && EntSysType.IsAssignableFrom(t));


        foreach (var sysT in entSyss)
        {
            var sys = (EntitySystem?)Activator.CreateInstance(sysT);
            InitializedSystems.Add(sys!);
            if (verbouse)
                Logger.LogInfo($"Initializing system {sys!.GetType()} : {sys!.GetHashCode()}", true, ConsoleColor.DarkBlue);

        }

        foreach (var sys in InitializedSystems)
        {
            InjectDependencies(sys, verbouse);
            sys.Init();
        }
        // foreach (var sys in InitializedSystems)
        // {
        //     sys.Init();
        // }
    }

    public void InjectDependencies<T>(T system, bool v = false)
    {
        var allInjectableDependency = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<InjectableDependency>() != null && typeof(EntitySystem).IsAssignableFrom(t));

        var fields = system!.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<SystemDependency>() is not null)
            {
                // foreach (var initializedSystem in InitializedSystems)
                // {
                //     if (initializedSystem.GetType() == field.FieldType)
                //     {
                //         field.SetValue(system, initializedSystem);
                //         if (v)
                //             Logger.LogInfo($"Injecting dependency {initializedSystem} : {initializedSystem.GetHashCode()} into {system}.{field.Name}");
                //     }
                // }
                foreach (var _classT in allInjectableDependency)
                {
                    if (_classT == field.FieldType)
                    {
                        // Search for existing instance
                        var instance = InitializedSystems.Find(sys => sys.GetType() == _classT);

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
}

public static class Logger
{
    public static void LogInfo(object? msg, bool fancy = false, ConsoleColor color = new ConsoleColor())
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[INFO] ");
        Console.ResetColor();
        if (fancy)
            PrintFancy($"{msg}", color);
        else
            Console.Write(msg + "\n");
    }
    public static void LogError(object? msg, bool fancy = false, ConsoleColor color = new ConsoleColor())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[ERR] ");
        Console.ResetColor();
        if (fancy)
            PrintFancy($"{msg}", color);
        else
            Console.Write(msg + "\n");
    }

    private static void PrintFancy(string msg, ConsoleColor color)
    {
        foreach (var str in msg.Split(" "))
        {
            try
            {
                Convert.ToDouble($"{str} ");
                Console.ForegroundColor = color;
                Console.Write($"{str} ");
                Console.ResetColor();
            }
            catch
            {
                Console.Write($"{str} ");
            }

        }
        Console.Write("\n");
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