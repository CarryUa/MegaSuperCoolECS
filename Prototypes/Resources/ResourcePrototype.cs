namespace ECS.Prototypes.Resources;

public class ResourcePrototype : IPrototype, IResource
{
    public string Type { get; set; } = "";
    public string Id { get; set; } = "";

    public string ResourcePath { get; set; } = "";
}

public interface IResource
{
    public string ResourcePath { get; set; }

}