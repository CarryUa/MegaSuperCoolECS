using ECS.System;
using System.Runtime.InteropServices;
using ECS.Logs;
using ECS.Prototypes.Resources;
using ECS.Prototypes;
using System.Diagnostics;

namespace ECS.Resources;

enum ColorType : byte
{
    Grayscale = 0,
    Truecolor = 2,
    Indexed = 3,
    GrayscaleAlpha = 4,
    TruecolorAlpha = 6
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct PNGImage
{
    public uint Width;
    public uint Height;
    public byte Bit_depth;
    public byte Bytes_per_pixel;
    public ColorType Color_type;
    public IntPtr Pixel_data;
    public UIntPtr Pixel_data_len; // <-- added to match native struct
};

[NeedDependencies]
public class ResourceManager
{
    private readonly List<IResource> Resources = new();

    [SystemDependency] private readonly PrototypeManager _protoman = default!;

    [DllImport("pngdecoder.dll", CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Ansi, ExactSpelling = true, EntryPoint = "read_image")]
    private static extern PNGImage read_image([MarshalAs(UnmanagedType.LPStr)] string filename);

    public List<ImageResourcePrototype> LoadAllPrototypes()
    {
        Logger.LogInfo($"Is 64bit: {Environment.Is64BitProcess}");
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<ImageResourcePrototype> protos = new List<ImageResourcePrototype>();

        var resFiles = Directory.EnumerateFiles("./", "*.png", SearchOption.AllDirectories);

        foreach (var file in resFiles)
        {

            var proto = new ImageResourcePrototype(Path.GetFileNameWithoutExtension(file));
            if (proto is null) continue;
            Logger.PrintQueue();

            try
            {
                PNGImage output = read_image(file);
                byte[] data = Array.Empty<byte>();
                if (output.Pixel_data != IntPtr.Zero && output.Pixel_data_len != UIntPtr.Zero)
                {
                    data = new byte[output.Pixel_data_len];
                    Marshal.Copy(output.Pixel_data, data, 0, (int)output.Pixel_data_len);
                }

                proto.BitDepth = output.Bit_depth;
                proto.BytesPerPixel = output.Bytes_per_pixel;
                proto.PixelData = data;
                proto.Height = (int)output.Height;
                proto.Width = (int)output.Width;
                proto.ResourcePath = file;
                proto.Type = "ImageResourcePrototype";

                protos.Add(proto);
                Resources.Add(proto);
            }
            catch (StackOverflowException e)
            {
                Logger.LogFatal($"{e.Message}");
            }
        }
        stopwatch.Stop();
        Logger.LogInfo($"Loaded {Resources.Count} images in {stopwatch.Elapsed.Seconds}s", true, ConsoleColor.Green);

        return protos;
    }

    public bool TryGetResource<TRes>(string resourcePath, out TRes? res)
    where TRes : IResource
    {
        res = default;

        res = (TRes?)Resources.FirstOrDefault(r => r.ResourcePath == resourcePath);

        return res is not null;
    }

};