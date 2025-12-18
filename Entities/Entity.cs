using ECS.Components;
using ECS.Logs;

namespace ECS.Entities;

public interface IEntity
{
    /// <summary>
    /// The unique identifier of this entity.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// List of components attached to given entity.
    /// </summary>
    List<IComponent> Components { get; }

    /// <summary>
    /// Attaches component to this entity.
    /// </summary>
    /// <param name="comp">The component to be attached.</param>
    void AttachComponent(IComponent comp);

    /// <summary>
    /// Detaches component from this entity.
    /// </summary>
    /// <param name="comp">The component to be detached.</param>
    void DetachComponent(IComponent comp);
};

public class Entity : IEntity
{
    public int Id { get; }
    public List<IComponent> Components { get; }

    private int _id;
    private List<IComponent> _components = new List<IComponent>();

    public string Name { get; set; } = "";

    public Entity(int id)
    {
        Id = id;
        Components = _components;
    }

    public void AttachComponent(IComponent comp)
    {
        if (!_components.Contains(comp))
        {
            Logger.LogDebug($"Attaching component {comp} to {this}", true, ConsoleColor.DarkGreen);

            _components.Add(comp);
            comp.OwnerID = this._id;
        }
    }
    public void DetachComponent(IComponent comp)
    {
        Logger.LogDebug($"Detaching component {comp} from {this}", true, ConsoleColor.DarkGreen);
        _components.Remove(comp);
        comp.OwnerID = -1;
    }

    public override string ToString()
    {
        return $"Entity({Id}) with {Components.Count} components";
    }
};