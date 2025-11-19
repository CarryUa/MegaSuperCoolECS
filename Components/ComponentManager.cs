using System.Reflection;
using ECS.Entities;
using ECS.Events;
using ECS.Events.ComponentEvents;
using ECS.Logs;
using ECS.System;
namespace ECS.Components;

/// <summary>
/// Manages creation, cloning and storage of all components.
/// </summary>
[NeedDependencies]
public class ComponentManager
{
    [SystemDependency] private readonly EventManager _evMan = default!;
    [SystemDependency] private readonly EntityManager _entMan = default!;

    /// <summary>
    /// List of all created components.
    /// </summary>
    public List<IComponent> Components { get => _components; }
    private List<IComponent> _components = new List<IComponent>();


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

        var newComp = Activator.CreateInstance(typeof(TComp), newId);
        _components.Add((IComponent)newComp!);

        var ev = new ComponentCreatedEvent((IComponent)newComp!);
        _evMan.RaiseEvent(ev);
        return (TComp)newComp!;
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

        var ev = new ComponentCreatedEvent((IComponent)newComp!);
        _evMan.RaiseEvent(ev);
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

        var ev = new ComponentClonedEvent(newComp, comp);
        _evMan.RaiseEvent(ev);
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
        foreach (var field in newComp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            field.SetValue(newComp, field.GetValue(comp));
        }

        // Go over all the properties of new copy, and set it's values to other's
        foreach (var prop in newComp.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (!prop.CanWrite) continue;
            prop.SetValue(newComp, prop.GetValue(comp));
        }

        var ev = new ComponentClonedEvent(newComp, comp);
        _evMan.RaiseEvent(ev);
        return newComp;
    }

    public bool HasComp<TComp>(int id)
    where TComp : IComponent
    {
        var ent = _entMan.GetEntityById(id);
        if (ent is null) return false;
        if (ent.Components.Any(c => c.GetType() == typeof(TComp))) return true;
        return false;
    }
    public bool HasComp<TComp>(IEntity ent)
    where TComp : IComponent
    {
        if (ent is null) return false;
        if (ent.Components.Any(c => c.GetType() == typeof(TComp))) return true;
        return false;
    }

    public bool TryGetComp<TComp>(int id, out TComp? component)
    where TComp : IComponent
    {
        component = default;
        var ent = _entMan.GetEntityById(id);
        if (ent is null) return false;

        component = (TComp?)ent.Components.FirstOrDefault(c => c.GetType() == typeof(TComp));
        return component is not null;
    }

    public bool TryGetComp<TComp>(IEntity ent, out TComp? component)
    where TComp : IComponent
    {
        component = default;
        if (ent is null) return false;

        component = (TComp?)ent.Components.FirstOrDefault(c => c.GetType() == typeof(TComp));
        return component is not null;
    }
}