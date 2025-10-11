using ECS.Components;
using ECS.Prototypes;
using ECS.Prototypes.Entities;
using ECS.System;
namespace ECS.Entities;

[NeedDependencies]
public class EntityManager
{
    [SystemDependency] private readonly PrototypeManager _protoMan = default!;
    [SystemDependency] private readonly CompManager _compMan = default!;

    public List<IEntity> Entities
    {
        get => _entities;
    }

    private List<IEntity> _entities = new List<IEntity>();
    private int _nextId = 1;

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

    public void RemoveEntity(IEntity entity)
    {
        _entities.Remove(entity);
    }

    public IEntity? GetEntityById(int id)
    {
        return _entities.FirstOrDefault(e => e.Id == id);
    }
}