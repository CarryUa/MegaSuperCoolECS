using ECS.Prototypes.Resources;
using ECS.Prototypes.Shaders;

namespace ECS.Components.Sprite;

public class SpriteComponent() : Component
{
    public string SpritePath = "";

    public string ShaderProgramProtoID = "";

    public ShaderProgramPrototype? ShaderProgram;

    public IImageResource? Image = null;

    // public TimeSpan? NextUpdateTime = null;

    // public ImageFrame<Rgba32>? CurrentFrame = null;

    // public float AnimationFPS = 1;

    // public int CurrentFrameIndex = 0;

    // public int TextureID { get; set; }
}