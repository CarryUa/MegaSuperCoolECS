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

    public override void PreInit()
    {
        base.PreInit();
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
                var cached = _loadedSprites[comp.SpritePath];
                comp.Image = cached.Image;
                comp.CurrentFrame = cached.CurrentFrame;
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
            comp.CurrentFrame = (ImageFrame<Rgba32>)comp.Image.Frames[0];




            // Handle OpenTK binding.
            if (comp.TextureID == 0) throw new IndexOutOfRangeException($"GL context couldn't generate texture");

            byte[] data = new byte[comp.CurrentFrame.Width * comp.CurrentFrame.Height * 4];

            comp.CurrentFrame.CopyPixelDataTo(data);

            GL.BindTexture(TextureTarget.Texture2D, comp.TextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, comp.Image.Width, comp.Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            data = [];

            LogInfo($"Loaded sprite from '{comp.SpritePath}': {comp.Image.Size.Height * comp.Image.Size.Width}bytes over {comp.Image.Frames.Count} frames. Texture ID: {comp.TextureID} ", true);

            _loadedSprites.TryAdd(comp.SpritePath, comp);
        }
        catch (Exception ex)
        {
            LogError($"Couldn't load sprite from '{comp.SpritePath}'. {ex.Message}");
        }
    }

    public void UpdateSprite(SpriteComponent comp)
    {
        // LogInfo($"Trying to start updateSprite");
        if (comp.Image is null)
        {
            LogError($"Tried to load empty texture with id {comp.TextureID} on entity {comp.OwnerID} with component {comp.Id}");
            return;
        }
        if (comp.Image.Frames.Count <= 1) return;


        comp.NextUpdateTime ??= TimeSpan.FromSeconds(1f / comp.AnimationFPS);

        if (_time.Time >= comp.NextUpdateTime)
        {
            // LogInfo($"Trying to update image");
            comp.NextUpdateTime += TimeSpan.FromSeconds(1f / comp.AnimationFPS);
            comp.CurrentFrameIndex++;
            if (comp.CurrentFrameIndex >= comp.Image.Frames.Count)
                comp.CurrentFrameIndex = 0;

            comp.CurrentFrame = (ImageFrame<Rgba32>)comp.Image.Frames[comp.CurrentFrameIndex];

            byte[] data = new byte[comp.CurrentFrame!.Height * comp.CurrentFrame.Width * 4 * sizeof(byte)];
            comp.CurrentFrame.CopyPixelDataTo(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, comp.TextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, comp.Image.Width, comp.Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            data = [];
        }
    }
}