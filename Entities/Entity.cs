using ECS.Components;

namespace ECS.Entities;

public class Entity
{
    public Entity(List<Component> components)
    {
        Components = components;
    }

    public List<Component> Components { get; set; } = [];

};