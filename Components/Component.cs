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