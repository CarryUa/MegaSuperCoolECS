using OpenTK.Graphics.OpenGL4;

namespace ECS.Prototypes.Resources;

public class ImageResourcePrototype : ResourcePrototype, IImageResource
{

    public ImageResourcePrototype(string id)
    {
        this.Id = id;
    }
    public int TextureID { get; set; }
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
    public byte[] PixelData { get; set; } = [];
    public byte BitDepth { get; set; }
    public byte BytesPerPixel { get; set; }
    // public ColorType Color_type;
}

public interface IImageResource : IResource
{
    public int TextureID { get; set; }
    public byte[] PixelData { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public byte BitDepth { get; set; }
    public byte BytesPerPixel { get; set; }
}