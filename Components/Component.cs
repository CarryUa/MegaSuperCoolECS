namespace ECS.Components;

/// <summary>
/// Base interface for all components.
/// </summary>
public interface IComponent
{
    /// <summary>
    /// The unique identifier of this component.
    /// </summary>
    int Id { get; set; }

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
public class Component() : IComponent
{
    public int Id { get; set; } = -1;

    public int OwnerID { get; set; }


    public override string ToString()
    {
        return $"{GetType().Name}({Id})";
    }

};