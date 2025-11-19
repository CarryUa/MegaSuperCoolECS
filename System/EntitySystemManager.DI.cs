// DI stands for Dependency Injection
using System.Collections.Concurrent;
using System.Reflection;
using ECS.Logs;

namespace ECS.System;

public partial class EntitySystemManager
{
    public List<object> InjectableInstances { get => _injectableInstances.ToList(); }

    /// <summary>
    /// List for instaces that can be injected including systems.
    /// AKA with <see cref="NeedDependencies"/> attribute
    /// </summary>
    private ConcurrentBag<object> _injectableInstances = [];

    /// <summary>
    /// Injects all dependencies into the given object.
    /// </summary>
    /// <param name="obj">The object needing dependencies.</param>
    /// <param name="v">Verbose logging flag.</param>
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
        Thread.EndThreadAffinity();
        return;
    }
}