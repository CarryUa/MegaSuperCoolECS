using ECS.Components.Sprite;
using ECS.Events.ComponentEvents;
using ECS.System.Time;
using MyOpenTKWindow;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ECS.System.Sprite;

[InitializationPriority(InitPriority.High)]
public class SpriteSystem : EntitySystem
{
    private Dictionary<string, SpriteComponent> _loadedSprites = [];

    [SystemDependency] private readonly MyWindow _window = default!;
    [SystemDependency] private readonly TimeSystem _time = default!;

    public override void Init()
    {
        base.Init();
        SubscribeEvent<SpriteComponent, ComponentClonedEvent>(OnComponentCloned);
    }

    public void OnComponentCloned(SpriteComponent comp, ComponentClonedEvent ev)
    {
        if (ev.Component != comp) return;

        ResolveSprite(comp);


    }
    public void ResolveSprite(SpriteComponent comp)
    {
        try
        {
            // Load image file and read data
            if (_loadedSprites.ContainsKey(comp.SpritePath))
            {
                comp = _loadedSprites[comp.SpritePath];
                LogInfo($"Loaded sprite from '{comp.SpritePath}': {comp.Image!.Size.Height * comp.Image.Size.Width}bytes over {comp.Image.Frames.Count} frames. Texture ID: {comp.TextureID} ", true, ConsoleColor.Green);

                return;
            }
            else
            {
                var stream = File.Open(comp.SpritePath, FileMode.Open, FileAccess.Read);
                comp.Image = Image.Load(stream);
                stream.Close();

                comp.TextureID = _window.RequestTexture();
            }

            if (comp is null) throw new NullReferenceException("SpriteComponent data was not loaded");

            // Invert image by X axis
            // comp.Image.Mutate(t => t.Flip(FlipMode.Vertical));
            var frame = (ImageFrame<Rgba32>)comp.Image.Frames[0];

            // Transform Image object to byte[] buffer
            byte[] data = new byte[frame.Width * frame.Height * 4];
            frame.CopyPixelDataTo(data);
            comp.PixelDataBuffer = data;

            // Handle OpenTK binding.
            if (comp.TextureID == 0) throw new IndexOutOfRangeException($"GL context couldn't generate texture");

            GL.BindTexture(TextureTarget.Texture2D, comp.TextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, comp.Image.Width, comp.Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, comp.PixelDataBuffer);
            LogInfo($"Loaded sprite from '{comp.SpritePath}': {comp.Image.Size.Height * comp.Image.Size.Width}bytes over {comp.Image.Frames.Count} frames. Texture ID: {comp.TextureID} ", true);

            if (!_loadedSprites.ContainsKey(comp.SpritePath))
                _loadedSprites.Add(comp.SpritePath, comp);

        }
        catch (Exception ex)
        {
            LogError($"Couldn't load sprite from '{comp.SpritePath}'. {ex.Message}");
        }
    }

    public void UpdateSprite(SpriteComponent comp)
    {
        if (comp.Image is null)
        {
            LogError($"Tried to load empty texture with id {comp.TextureID} on entity {comp.OwnerID} with component {comp.Id}");
            return;
        }
        if (comp.Image.Frames.Count <= 1) return;


        comp.NextUpdateTime ??= TimeSpan.FromSeconds(1f / comp.AnimationFPS);

        if (_time.Time >= comp.NextUpdateTime)
        {
            comp.NextUpdateTime += TimeSpan.FromSeconds(1f / comp.AnimationFPS);
            comp.CurrentFrameIndex++;
            if (comp.CurrentFrameIndex >= comp.Image.Frames.Count)
                comp.CurrentFrameIndex = 0;

            ((ImageFrame<Rgba32>)comp.Image.Frames[(int)comp.CurrentFrameIndex]).CopyPixelDataTo(comp.PixelDataBuffer);

            GL.BindTexture(TextureTarget.Texture2D, comp.TextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, comp.Image.Width, comp.Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, comp.PixelDataBuffer);

        }
    }
}