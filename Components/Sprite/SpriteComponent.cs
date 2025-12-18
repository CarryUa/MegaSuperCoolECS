
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ECS.Components.Sprite;

public class SpriteComponent() : Component
{
    public string SpritePath = "";

    public Image? Image = null;

    public TimeSpan? NextUpdateTime = null;

    public ImageFrame<Rgba32>? CurrentFrame = null;

    public float AnimationFPS = 1;

    public int CurrentFrameIndex = 0;

    public int TextureID { get; set; }
}