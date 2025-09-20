using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MyOpenTKWindow;
class MyWindow : GameWindow
{
    public static int SCREENWIDTH;
    public static int SCREENHEIGHT;

    public event Action<double> UpdateSystems;

    public MyWindow(int w, int h) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        SCREENHEIGHT= h;
        SCREENWIDTH = w;
        this.CenterWindow(new(w,h));
    }
    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
    }
    protected override void OnUnload()
    {
        base.OnUnload();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        UpdateSystems.Invoke(this.TimeSinceLastUpdate());

    }

}
