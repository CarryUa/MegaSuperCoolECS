namespace MegaSuperCoolECS.ECS;

public class Component(int newId)
{
    public int id
    {
        get => _id;
    }

    protected int _id = newId;

};

public class CompManager() : EntitySystem
{
    public List<Component> Components { get => _components; }
    private List<Component> _components = [];

    public Component CreateComponent<TComp>() where TComp : Component
    {
        var newId = _components.Count;

        var comp = Activator.CreateInstance(typeof(TComp), newId);
        _components.Add((Component)comp!);
        Logger.LogInfo($"Creating new component of type {comp!.GetType()} : {comp.GetHashCode()} with Component.id {newId}", true, ConsoleColor.DarkBlue);

        return (Component)comp!;
    }
}