using ECS.Prototypes.Shaders;

namespace ECS.Events.ShaderEvents;

public class ShaderProgramPreRenderEvent : Event
{
    /// <summary>
    /// The shader program being rendered.
    /// </summary>
    public required ShaderProgramPrototype ShaderProgramPrototype;
}