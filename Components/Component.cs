using ECS.System;
using ECS.Logs;
using System.Reflection;

namespace ECS.Components;

/// <summary>
/// Base interface for all components.
/// </summary>
public interface IComponent
{
    int Id { get; }
}

/// <summary>
/// Base class for all components. Implements <see cref="IComponent"/>.
/// </summary>
/// <remarks>
/// All components should inherit from this class.
/// </remarks>
/// <param name="newId"></param>
public class Component(int newId) : IComponent
{
    public int Id
    {
        get => _id;
    }

    protected int _id { get; } = newId;
};

/// <summary>
/// Manages creation, cloning and storage of all components.
/// </summary>
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

    /// <summary>
    /// Creates a new component of type <typeparamref name="TComp"/> and adds it to the list of all components.
    /// </summary>
    /// <typeparam name="TComp">The type of the component to create.</typeparam>
    /// <returns>The created component.</returns>
    public TComp CreateComponent<TComp>() where TComp : IComponent
    {
        var newId = _components.Count;

        var comp = Activator.CreateInstance(typeof(TComp), newId);
        _components.Add((IComponent)comp!);
        Logger.LogInfo($"Creating new component of type {comp!.GetType()} : {comp.GetHashCode()} with Component.id {newId}", true, ConsoleColor.DarkBlue);

        return (TComp)comp!;
    }

    /// <summary>
    /// Creates a new component of type specified by <paramref name="compType"/> and adds it to the list of all components.
    /// </summary>
    /// <param name="compType">The type of the component to create.</param>
    /// <returns>The created component.</returns>
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

    /// <summary>
    /// Gets the type of component by its name.
    /// </summary>
    /// <remarks>
    /// Exception is thrown when the component type is not found.
    /// </remarks>
    /// <param name="typeName">The name of the component type.</param>
    /// <returns>The type of the component.</returns>
    /// <exception cref="NullReferenceException"></exception>
    public Type GetComponentType(string typeName)
    {
        var type = _componentTypes.FirstOrDefault(t => t.Name == typeName);
        if (type is null)
            throw new NullReferenceException($"Component type '{typeName}' not found.");
        return type;
    }

    /// <summary>
    /// Gets the type of component by its name.
    /// </summary>
    /// <param name="typeName">The name of the component type.</param>
    /// <param name="type">The type of the component or null if not found.</param>
    /// <returns>True if type is found, false otherwise.</returns>
    public bool TryGetComponentType(string typeName, out Type? type)
    {
        type = _componentTypes.FirstOrDefault(t => t.Name == typeName);
        return type is not null;
    }

    /// <summary>
    /// Clones the component data into the new instance.
    /// </summary>
    /// <typeparam name="TComp">The type of the component to clone.</typeparam>
    /// <param name="comp">The component to be cloned.</param>
    /// <returns>The cloned component.</returns>
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

    /// <summary>
    /// Clones the component data into the new instance.
    /// </summary>
    /// <param name="comp">The component to be cloned.</param>
    /// <returns>The cloned component.</returns>
    public IComponent CloneComponent(IComponent comp)
    {
        var newType = _componentTypes.FirstOrDefault(t => t.Name == comp.GetType().Name);
        var newComp = CreateComponent(newType!);
        // Go over all the fields of new copy, and set it's values to other's
        foreach (var field in newComp.GetType().GetFields())
        {
            field.SetValue(newComp, field.GetValue(comp));
        }

        return newComp;
    }
}