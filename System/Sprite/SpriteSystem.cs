using ECS.Components.Sprite;
using ECS.Events.ComponentEvents;
using ECS.Prototypes;
using ECS.Prototypes.Resources;
using ECS.Prototypes.Shaders;
using ECS.Resources;
using ECS.System.Time;
using MyOpenTKWindow;
using OpenTK.Graphics.OpenGL4;

namespace ECS.System.Sprite;

[InitializationPriority(InitPriority.High)]
public class SpriteSystem : EntitySystem
{
    private Dictionary<string, SpriteComponent> _loadedSprites = [];

    [SystemDependency] private readonly MyWindow _window = default!;
    [SystemDependency] private readonly TimeSystem _time = default!;
    [SystemDependency] private readonly ResourceManager _resMan = default!;
    [SystemDependency] private readonly PrototypeManager _protoMan = default!;


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
            comp.ShaderProgram = _protoMan.GetPrototype<ShaderProgramPrototype>(comp.ShaderProgramProtoID);

            if (!_resMan.TryGetResource<ImageResourcePrototype>(comp.SpritePath, out var res))
            {
                throw new NullReferenceException($"Resource at '{comp.SpritePath}' was not found or loaded");
            }

            comp.Image = res!;

            LogInfo($"Image data:\n\n size: {comp.Image.Width}x{comp.Image.Height}px\ndata length: {comp.Image.PixelData.Length}");
            comp.Image!.TextureID = _window.RequestTexture();


            if (comp is null || comp.Image.PixelData is null) throw new NullReferenceException("SpriteComponent data was not loaded");


            // Handle OpenTK binding.
            if (comp.Image.TextureID == 0) throw new IndexOutOfRangeException($"GL context couldn't generate texture");

            GL.BindTexture(TextureTarget.Texture2D, comp.Image.TextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, comp.Image.Width, comp.Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, comp.Image.PixelData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            LogInfo($"Loaded sprite from '{comp.SpritePath}': {comp.Image.Height * comp.Image.Width}bytes. Texture ID: {comp.Image.TextureID} ", true);

            _loadedSprites.TryAdd(comp.SpritePath, comp);
        }
        catch (Exception ex)
        {
            LogError($"Couldn't load sprite from '{comp.SpritePath}'. {ex.Message}");
            throw;
        }
    }

    // public void UpdateSprite(SpriteComponent comp)
    // {
    //     // LogInfo($"Trying to start updateSprite");
    //     if (comp.Image is null)
    //     {
    //         LogError($"Tried to load empty texture with id {comp.TextureID} on entity {comp.OwnerID} with component {comp.Id}");
    //         return;
    //     }
    //     if (comp.Image.Frames.Count <= 1) return;


    //     comp.NextUpdateTime ??= TimeSpan.FromSeconds(1f / comp.AnimationFPS);

    //     if (_time.Time >= comp.NextUpdateTime)
    //     {
    //         // LogInfo($"Trying to update image");
    //         comp.NextUpdateTime += TimeSpan.FromSeconds(1f / comp.AnimationFPS);
    //         comp.CurrentFrameIndex++;
    //         if (comp.CurrentFrameIndex >= comp.Image.Frames.Count)
    //             comp.CurrentFrameIndex = 0;

    //         comp.CurrentFrame = (ImageFrame<Rgba32>)comp.Image.Frames[comp.CurrentFrameIndex];

    //         byte[] data = new byte[comp.CurrentFrame!.Height * comp.CurrentFrame.Width * 4 * sizeof(byte)];
    //         comp.CurrentFrame.CopyPixelDataTo(data);

    //         GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    //         GL.BindTexture(TextureTarget.Texture2D, comp.TextureID);
    //         GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, comp.Image.Width, comp.Image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    //         data = [];
    //     }
    // }
}