using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MyOpenTKWindow;

class MyWindow : GameWindow
{
    public static int SCREENWIDTH;
    public static int SCREENHEIGHT;

    public event Action<double> UpdateSystems;

    /// <summary>
    ///  
    /// </summary>
    /// <param name="w">Width of the screen</param>
    /// <param name="h">Height of the screen</param>
    /// <param name="updateAction">Action to be called to update systems</param>
    public MyWindow(int w, int h, Action<double> updateAction) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        SCREENHEIGHT = h;
        SCREENWIDTH = w;
        UpdateSystems += updateAction;
        this.CenterWindow(new(w, h));
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
