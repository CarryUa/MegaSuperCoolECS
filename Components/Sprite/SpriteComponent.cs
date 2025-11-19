
using SixLabors.ImageSharp;

namespace ECS.Components.Sprite;

public class SpriteComponent(int newId) : Component(newId)
{
    public string SpritePath = "";

    public Image? Image = null;

    public TimeSpan? NextUpdateTime = null;

    public float AnimationFPS = 1;

    public byte[] PixelDataBuffer = [];

    public uint CurrentFrameIndex = 0;

    public int TextureID { get; set; }
}