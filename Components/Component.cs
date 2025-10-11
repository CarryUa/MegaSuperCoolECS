using ECS.System;
using ECS.Logs;
using System.Reflection;

namespace ECS.Components;

public interface IComponent
{
    int Id { get; }
}

public class Component(int newId) : IComponent
{
    public int Id
    {
        get => _id;
    }

    protected int _id { get; } = newId;
};

[NeedDependencies]
public class CompManager()
{
    /// <summary>
    /// List of all created components.
    /// </summary>
    public List<IComponent> Components { get => _components; }
    private List<IComponent> _components = [];


    /// <summary>
    /// List of all available component types in the assembly.
    /// </summary>
    public List<Type> ComponentTypes { get => _componentTypes; }
    // TODO: Get components from EntitySystemManager instead of searching assembly
    private List<Type> _componentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IComponent).IsAssignableFrom(t) && !t.IsAbstract).ToList();

    public TComp CreateComponent<TComp>() where TComp : IComponent
    {
        var newId = _components.Count;

        var comp = Activator.CreateInstance(typeof(TComp), newId);
        _components.Add((IComponent)comp!);
        Logger.LogInfo($"Creating new component of type {comp!.GetType()} : {comp.GetHashCode()} with Component.id {newId}", true, ConsoleColor.DarkBlue);

        return (TComp)comp!;
    }
    public IComponent CreateComponent(Type compType)
    {
        if (!typeof(IComponent).IsAssignableFrom(compType))
            throw new InvalidCastException($"Tried to convert {compType} to {typeof(IComponent)}");


        var newId = _components.Count;

        var newComp = Activator.CreateInstance(compType, newId);
        _components.Add((IComponent)newComp!);
        Logger.LogInfo($"Creating new component of type {newComp!.GetType()} : {newComp.GetHashCode()} with Component.id {newId}", true, ConsoleColor.DarkBlue);

        return (IComponent)newComp!;
    }

    public Type GetComponentType(string typeName)
    {
        var type = _componentTypes.FirstOrDefault(t => t.Name == typeName);
        if (type is null)
            throw new Exception($"Component type '{typeName}' not found.");
        return type;
    }

    public bool TryGetComponentType(string typeName, out Type? type)
    {
        type = _componentTypes.FirstOrDefault(t => t.Name == typeName);
        return type is not null;
    }

    public TComp CloneComponent<TComp>(TComp comp)
    where TComp : IComponent
    {
        var newComp = CreateComponent<TComp>();
        // Go over all the fields of new copy, and set it's values to other's
        foreach (var field in newComp.GetType().GetFields())
        {
            field.SetValue(newComp, field.GetValue(comp));
        }

        return newComp;
    }
    public IComponent CloneComponent(IComponent comp)
    {
        var newType = _componentTypes.FirstOrDefault(t => t.Name == comp.GetType().Name);
        var newComp = CreateComponent(newType);
        // Go over all the fields of new copy, and set it's values to other's
        foreach (var field in newComp.GetType().GetFields())
        {
            field.SetValue(newComp, field.GetValue(comp));
        }

        return newComp;
    }
}