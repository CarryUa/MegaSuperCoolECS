namespace ECS.Components;

/// <summary>
/// Base interface for all components.
/// </summary>
public interface IComponent
{
    /// <summary>
    /// The unique identifier of this component.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// The ID of the entity that owns this component.
    /// </summary>
    int OwnerID { get; set; }
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

    public int OwnerID { get; set; }

    protected int _id { get; } = newId;
};