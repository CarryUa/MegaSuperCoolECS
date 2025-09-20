namespace MegaSuperCoolECS.ECS;

public class Entity
{
    public Entity(List<Component> components)
    {
        Components = components;
    }

    public List<Component> Components { get; set; } = [];

};