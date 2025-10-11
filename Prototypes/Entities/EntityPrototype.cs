
using ECS.Components;

namespace ECS.Prototypes.Entities;

public class EntityPrototype : IPrototype
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";

    public string Name { get; set; } = "Unnamed Entity";

    public List<IComponent> Components { get; set; } = [];

    public override string ToString()
    {
        return $"{Type}({Id})\n\tComponents: [{string.Join(", ", Components)}]";
    }
}