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

        var newComp = (TComp)Activator.CreateInstance(typeof(TComp))!;
        newComp.Id = newId;
        _components.Add((IComponent)newComp);

        var ev = new ComponentCreatedEvent((IComponent)newComp);
        _evMan.RaiseEvent(ev);
        return (TComp)newComp;
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

        var newComp = (IComponent)Activator.CreateInstance(compType)!;
        newComp!.Id = newId;
        _components.Add((IComponent)newComp);

        var ev = new ComponentCreatedEvent((IComponent)newComp);
        _evMan.RaiseEvent(ev);
        return (IComponent)newComp;
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
    /// <param name="OwnerId">The owner of the new component. Default is -1 (no owner).</param>
    /// <returns>The cloned component.</returns>
    public TComp CloneComponent<TComp>(TComp comp, int OwnerId = -1)
    where TComp : IComponent
    {
        var copy = CreateComponent(comp.GetType());


        DeepCloneComponent(comp, ref copy);
        copy.OwnerID = OwnerId;

        var ev = new ComponentClonedEvent(copy, comp);
        _evMan.RaiseEvent(ev);
        return (TComp)copy;
    }

    /// <summary>
    /// Clones the component data into the new instance.
    /// </summary>
    /// <param name="comp">The component to be cloned.</param>
    /// <param name="OwnerId">The owner of the new component. Default is -1 (no owner).</param>
    /// <returns>The cloned component.</returns>
    public IComponent CloneComponent(IComponent comp, int OwnerId = -1)
    {
        var copy = CreateComponent(comp.GetType());

        DeepCloneComponent(comp, ref copy);
        copy.OwnerID = OwnerId;

        var ev = new ComponentClonedEvent(copy, comp);
        _evMan.RaiseEvent(ev);
        return copy;
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

    private void DeepCloneComponent(IComponent original, ref IComponent copy)
    {
        var fields = original.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (original == copy) return;

        Logger.LogInfo($"DeepCloning component {original} to {copy}");

        foreach (var f in fields)
        {
            var value = f.GetValue(original);

            if (value is null)
            {
                Logger.LogInfo($"\t\t>>>Copying null field {f.Name}");
                f.SetValue(copy, value); // Set directly

                continue;
            }

            if (f.FieldType.IsValueType)
            {
                Logger.LogInfo($"\t\t>>>Copying value field {f.Name} with value {value}");
                f.SetValue(copy, value); // Set directly

                continue;
            }

            if (f.FieldType == typeof(string))
            {
                Logger.LogInfo($"\t\t>>>Copying string field {f.Name} with value {value}");
                f.SetValue(copy, string.Join("", value));
            }

            if (f.FieldType.IsByRef)
            {
                Logger.LogInfo($"\t\t>>>Copying ref field {f.Name} with value {value}");

                try
                {
                    var byrefcopy = Activator.CreateInstance(value.GetType()); // Try to set to new instance
                    f.SetValue(copy, byrefcopy);
                }
                catch (Exception e)
                {
                    Logger.LogError($"\t\t>>>Couldn't clone ref type {f.Name}({value}): {e.Message}");
                }

                continue;
            }
        }

        var props = original.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var p in props)
        {
            var value = p.GetValue(original);

            if (value is null)
            {
                Logger.LogInfo($"\t\t>>>Copying null field {p.Name}");
                p.SetValue(copy, value); // Set directly

                continue;
            }

            if (p.PropertyType.IsValueType)
            {
                Logger.LogInfo($"\t\t>>>Copying value field {p.Name} with value {value}");
                p.SetValue(copy, value); // Set directly

                continue;
            }

            if (p.PropertyType == typeof(string))
            {
                Logger.LogInfo($"\t\t>>>Copying string field {p.Name} with value {value}");
                p.SetValue(copy, string.Join("", value));
            }

            if (p.PropertyType.IsByRef)
            {
                Logger.LogInfo($"\t\t>>>Copying ref field {p.Name} with value {value}");

                try
                {
                    var byrefcopy = Activator.CreateInstance(value.GetType()); // Try to set to new instance
                    p.SetValue(copy, byrefcopy);
                }
                catch (Exception e)
                {
                    Logger.LogError($"\t\t>>>Couldn't clone ref type {p.Name}({value}): {e.Message}");
                }

                continue;
            }


        }
    }
}