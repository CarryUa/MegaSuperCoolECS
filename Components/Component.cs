using ECS.System;
using ECS.Logs;

namespace ECS.Components;

public interface IComponent
{
    int Id { get; }
}

public class Component(int newId) : IComponent
{
    public int Id
    {
        get => _id;
    }

    protected int _id = newId;

};

public class CompManager() : EntitySystem
{
    public static List<IComponent> Components { get => _components; }
    private static List<IComponent> _components = [];

    public static TComp CreateComponent<TComp>() where TComp : IComponent
    {
        var newId = _components.Count;

        var comp = Activator.CreateInstance(typeof(TComp), newId);
        _components.Add((IComponent)comp!);
        Logger.LogInfo($"Creating new component of type {comp!.GetType()} : {comp.GetHashCode()} with Component.id {newId}", true, ConsoleColor.DarkBlue);

        return (TComp)comp!;
    }
}