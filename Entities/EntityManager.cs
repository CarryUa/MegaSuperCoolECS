using ECS.Components;
using ECS.Prototypes;
using ECS.Prototypes.Entities;
using ECS.System;
namespace ECS.Entities;

[NeedDependencies]
public class EntityManager
{
    [SystemDependency] private readonly PrototypeManager _protoMan = default!;
    [SystemDependency] private readonly ComponentManager _compMan = default!;

    /// <summary>
    /// List of all known entities.
    /// </summary>
    public List<IEntity> Entities
    {
        get => _entities;
    }

    private List<IEntity> _entities = new List<IEntity>();
    /// <summary>
    /// The next availible unique identifier.
    /// </summary>
    private int _nextId = 1;

    /// <summary>
    /// Creates a new entity from its EntityPrototype.
    /// </summary>
    /// <param name="protoId">The id of the prototype to use.</param>
    /// <returns>The created entity.</returns>
    public IEntity CreateEntity(string protoId)
    {
        var proto = _protoMan.GetPrototype<EntityPrototype>(protoId)!;
        var entity = new Entity(_nextId++);

        entity.Name = proto?.Name ?? "Unnamed Entity";

        foreach (var comp in proto?.Components ?? [])
        {
            var concrete = comp.GetType();
            entity.AttachComponent(_compMan.CloneComponent(comp));
        }

        _entities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Deletes the entity.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    public void RemoveEntity(IEntity entity)
    {
        _entities.Remove(entity);
    }

    /// <summary>
    /// Finds an entity by it's id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The found entity, or null if not found.</returns>
    public IEntity? GetEntityById(int id)
    {
        return _entities.FirstOrDefault(e => e.Id == id);
    }
}