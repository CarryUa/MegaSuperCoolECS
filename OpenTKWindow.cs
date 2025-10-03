using ECS.Logs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MyOpenTKWindow;

class MyWindow : GameWindow
{
    public static int SCREENWIDTH;
    public static int SCREENHEIGHT;

    public Action<double> UpdateSystems;

    private Shader? Shader { get; set; }

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
        try
        {
            Shader = new Shader("Shaders/basic.vert", "Shaders/basic.frag");
        }
        catch (Exception ex)
        {
            Logger.LogFatal($"Failed to load shaders: {ex.Message} {ex.InnerException?.Message}");
            this.Close();
        }
        GL.ClearColor(0.2f, 0.2f, 0.2f, 1);

        float[] vertices = {
                -0.5f, -0.5f, 0.0f, //Bottom-left vertex
                 0.5f, -0.5f, 0.0f, //Bottom-right vertex
                 0.0f,  0.5f, 0.0f  //Top vertex
                        };

        var VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);

        var VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }
    protected override void OnUnload()
    {
        base.OnUnload();
        Shader!.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        UpdateSystems.Invoke(args.Time);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.ClearColor(0.2f, 0.2f, 0.2f, 1);

        Shader!.Use();
        // 3. now draw the object
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        this.SwapBuffers();
    }
}

public class Shader : IDisposable
{
    public int Handle;

    public int VertexShader;
    public int FragmentShader;

    private bool disposedValue = false;

    ~Shader()
    {
        if (!disposedValue)
        {
            Logger.LogError("Shader is not disposed after finalization!");
        }
    }

    public Shader(string vertPath, string fragPath)
    {
        // Get source code
        string vertSource = File.ReadAllText(vertPath);
        string fragSource = File.ReadAllText(fragPath);

        // Generate shaders
        VertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexShader, vertSource);

        FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentShader, fragSource);

        // Compile shaders
        GL.CompileShader(VertexShader);

        GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(VertexShader);
            Console.WriteLine(infoLog);
        }

        GL.CompileShader(FragmentShader);

        GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(FragmentShader);
            Console.WriteLine(infoLog);
        }

        // Create GPU program and attach shaders
        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, VertexShader);
        GL.AttachShader(Handle, FragmentShader);

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }

        // Detach and delete shaders once program is linked
        GL.DetachShader(Handle, VertexShader);
        GL.DetachShader(Handle, FragmentShader);
        GL.DeleteShader(FragmentShader);
        GL.DeleteShader(VertexShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
