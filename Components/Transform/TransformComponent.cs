using System.Numerics;

namespace ECS.Components.Transform;

public class TransformComponent(int id) : Component(id)
{
    public Vector2 Size = Vector2.Zero;
    public Vector2 Position = Vector2.Zero;
}